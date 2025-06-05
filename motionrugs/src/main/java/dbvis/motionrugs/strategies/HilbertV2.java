package dbvis.motionrugs.strategies;

import dbvis.motionrugs.data.DataPoint;
import java.util.Arrays;
import java.util.Comparator;

/**
 * Hilbert curve ordering strategy with dynamic starting corner adjustment
 */
public class HilbertV2 implements Strategy {

    private int hilbertOrder = 2;
    private int currentCorner = 0; // 0: bottom-left, 1: top-left, 2: bottom-right, 3: top-right
    private int framesSinceLastChange = Integer.MAX_VALUE; // Compteur pour limiter les changements
    private static final int MIN_FRAMES_BETWEEN_CHANGES = 10; // Minimum de frames avant un nouveau changement
    private double lastAngle = 0.0; // Dernier angle de direction du swarm
    private static final double ANGLE_THRESHOLD = Math.PI / 4; // Seuil pour détecter un virage (45 degrés)

    @Override
    public String getName() {
        return "Hilbert curve";
    }

    /**
     * Returns dataset in Hilbert ordering with dynamic starting corner
     *
     * @param unsorted dataset
     * @return sorted dataset
     */
    @Override
    public DataPoint[][] getOrderedValues(DataPoint[][] unsorted) {
        DataPoint[][] result = new DataPoint[unsorted.length][unsorted[0].length];

        for (int x = 0; x < unsorted.length; x++) {
            // Détection de virage pour la frame x
            if (x > 0) {
                updateStartingCorner(unsorted, x);
            }

            Integer[] idx = new Integer[unsorted[x].length];
            long[] hilbertValues = new long[unsorted[x].length];

            // Appliquer la transformation des coordonnées selon le coin de départ
            for (int y = 0; y < unsorted[x].length; y++) {
                idx[y] = y;
                int[] transformed = transformCoordinates((int) unsorted[x][y].getX(), (int) unsorted[x][y].getY(), hilbertOrder);
                hilbertValues[y] = encode(transformed[0], transformed[1], hilbertOrder);
            }

            // Trier selon l'ordre de Hilbert
            Arrays.sort(idx, new Comparator<Integer>() {
                @Override
                public int compare(final Integer o1, final Integer o2) {
                    return Long.compare(hilbertValues[o1], hilbertValues[o2]);
                }
            });

            // Construire le résultat
            for (int y = 0; y < unsorted[x].length; y++) {
                result[x][y] = unsorted[x][idx[y]];
            }
        }
        framesSinceLastChange++;
        return result;
    }

    /**
     * Détecte un virage et met à jour le coin de départ si nécessaire
     */
    private void updateStartingCorner(DataPoint[][] unsorted, int frame) {
        if (framesSinceLastChange < MIN_FRAMES_BETWEEN_CHANGES) {
            return; // Ne pas changer si le dernier changement est trop récent
        }

        // Calculer le centroïde et le déplacement
        double centroidX = 0.0, centroidY = 0.0;
        double prevCentroidX = 0.0, prevCentroidY = 0.0;
        int count = unsorted[frame].length;

        for (int y = 0; y < count; y++) {
            centroidX += unsorted[frame][y].getX();
            centroidY += unsorted[frame][y].getY();
            prevCentroidX += unsorted[frame - 1][y].getX();
            prevCentroidY += unsorted[frame - 1][y].getY();
        }
        centroidX /= count;
        centroidY /= count;
        prevCentroidX /= count;
        prevCentroidY /= count;

        // Calculer la direction du déplacement
        double dx = centroidX - prevCentroidX;
        double dy = centroidY - prevCentroidY;
        double angle = Math.atan2(dy, dx);

        // Vérifier si l'angle a changé significativement
        if (Math.abs(angle - lastAngle) > ANGLE_THRESHOLD) {
            // Déterminer le nouveau coin en fonction de la direction
            if (angle >= -Math.PI / 4 && angle < Math.PI / 4) {
                currentCorner = 2; // Vers la droite -> bas-droite
            } else if (angle >= Math.PI / 4 && angle < 3 * Math.PI / 4) {
                currentCorner = 3; // Vers le haut -> haut-droite
            } else if (angle >= 3 * Math.PI / 4 || angle < -3 * Math.PI / 4) {
                currentCorner = 1; // Vers la gauche -> haut-gauche
            } else {
                currentCorner = 0; // Vers le bas -> bas-gauche
            }
            framesSinceLastChange = 0; // Réinitialiser le compteur
            lastAngle = angle;
        }
    }

    /**
     * Transforme les coordonnées en fonction du coin de départ
     */
    private int[] transformCoordinates(int x, int y, int order) {
        int maxCoord = (1 << order) - 1; // Taille maximale de la grille
        switch (currentCorner) {
            case 0: // Bas-gauche (défaut)
                return new int[]{x, y};
            case 1: // Haut-gauche
                return new int[]{x, maxCoord - y}; // Réflexion verticale
            case 2: // Bas-droite
                return new int[]{maxCoord - x, y}; // Réflexion horizontale
            case 3: // Haut-droite
                return new int[]{maxCoord - x, maxCoord - y}; // Réflexion horizontale et verticale
            default:
                return new int[]{x, y};
        }
    }

    /**
     * Encode les coordonnées en valeur de Hilbert
     */
    public int encode(int x, int y, int r) {
        int mask = (1 << r) - 1;
        int hodd = 0;
        int heven = x ^ y;
        int notx = ~x & mask;
        int noty = ~y & mask;
        int temp = notx ^ y;

        int v0 = 0, v1 = 0;
        for (int k = 1; k < r; k++) {
            v1 = ((v1 & heven) | ((v0 ^ noty) & temp)) >> 1;
            v0 = ((v0 & (v1 ^ notx)) | (~v0 & (v1 ^ noty))) >> 1;
        }
        hodd = (~v0 & (v1 ^ x)) | (v0 & (v1 ^ noty));

        return interleaveBits(hodd, heven);
    }

    /**
     * Interleave les bits de deux valeurs
     */
    private int interleaveBits(int odd, int even) {
        int val = 0;
        int max = Math.max(odd, even);
        int n = 0;
        while (max > 0) {
            n++;
            max >>= 1;
        }

        for (int i = 0; i < n; i++) {
            int bitMask = 1 << i;
            int a = (even & bitMask) > 0 ? (1 << (2 * i)) : 0;
            int b = (odd & bitMask) > 0 ? (1 << (2 * i + 1)) : 0;
            val += a + b;
        }
        return val;
    }

    /**
     * Définit l'ordre de Hilbert
     */
    public void setHilbertOrder(int value) {
        this.hilbertOrder = value;
    }
}
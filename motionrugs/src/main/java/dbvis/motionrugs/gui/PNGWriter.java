/*
 * Copyright 2018 Juri Buchmueller <motionrugs@dbvis.inf.uni-konstanz.de>.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package dbvis.motionrugs.gui;

import dbvis.motionrugs.color.BinnedPercentileColorMapper;
import dbvis.motionrugs.data.DataPoint;
import java.awt.Color;
import java.awt.Graphics2D;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.util.logging.Level;
import java.util.logging.Logger;
import javax.imageio.ImageIO;

/**
 * PNGWriter is responsible for the creation of the visualization images. It
 * applies Colormaps and returns BufferedImages. Also, saves the resulting
 * images.
 *
 * @author Juri Buchmüller, University of Konstanz
 * <buchmueller@dbvis.inf.uni-konstanz.de>
 */
public class PNGWriter {

    // Facteur d'agrandissement verticale, peut être paramétré depuis l'ui
    private static int verticalScaleFactor = 2;
    
    public static void setVerticalScaleFactor(int factor) {
        if (factor >= 1) {
            verticalScaleFactor = factor;
        }
    }
    
    public static int getVerticalScaleFactor() {
        return verticalScaleFactor;
    }

    /**
     *
     * According to a chosen Colormapper, creates a BufferedImage of a Rugs
     * using one feature, saves it to the default project directory and returns
     * the image for display in the GUI
     *
     * @param da the array with ordered values
     * @param min min value of the feature values for the color mapping
     * @param max max value of the feature values for the color mapping
     * @param decs the percentiles (bins) for the colors (limited to 10 currently)
     * @param featureID the name of the displayed feature
     * @param dsname the name of the displayed dataset
     * @param stratid the name of the chosen strategy
     * @return the MotionRug created from the ordered data
     */
    public static BufferedImage drawAndSaveRugs(DataPoint[][] da, double min, double max, Double[] decs, String featureID, String dsname, String stratid) {
        int imageWidth = Math.max(da.length, 800);
        int imageHeight = da[0].length * verticalScaleFactor;
        
        BufferedImage awtImage = new BufferedImage(imageWidth, imageHeight, BufferedImage.TYPE_INT_RGB);
        Graphics2D g2d = awtImage.createGraphics();
        g2d.setColor(Color.WHITE);
        g2d.fillRect(0, 0, imageWidth, imageHeight);
        g2d.dispose();
        
        Color[] colors;
        Double[] thresholds;
        
        if (featureID.equals("DominantParameter")) {
            thresholds = new Double[]{0.0, 1.0, 2.0, 3.0, 4.0};
            // Associer des couleurs distinctes à chaque paramètre
            colors = new Color[]{
            	new Color(255, 255, 255),// Blanc si aucune dominance
                new Color(255, 0, 0),   // Rouge pour cohesion (1)
                new Color(0, 255, 0),   // Vert pour separation (2)
                new Color(0, 0, 255),   // Bleu pour alignement (3)
                new Color(255, 255, 0)  // Jaune pour bouncesOffWall (4)
            };
        } else {
            colors = new Color[]{
                new Color(49, 54, 149), new Color(69, 117, 180), new Color(116, 173, 209),
                new Color(171, 217, 233), new Color(224, 243, 248), new Color(254, 224, 144),
                new Color(253, 174, 97), new Color(244, 109, 67), new Color(215, 48, 39),
                new Color(165, 0, 38)
            };
            thresholds = decs;
        }
        
        BinnedPercentileColorMapper bqcm = new BinnedPercentileColorMapper(thresholds, min, max, colors);

        for (int x = 0; x < da.length; x++) {
            for (int y = 0; y < da[x].length; y++) {
                try {
                    if (da[x][y].getValue(featureID) < min) {
                        System.out.println("ERROR: " + featureID + " " + da[x][y].getValue(featureID) + "<" + min + ", id " + min + ", frame " + y);
                    }
                    
                    Color pixelColor = bqcm.getColorByValue(da[x][y].getValue(featureID));
                    
                    for (int i = 0; i < verticalScaleFactor; i++) {
                        awtImage.setRGB(x, y * verticalScaleFactor + i, pixelColor.getRGB());
                    }
                    
                } catch (Exception ex) {
                    System.out.println(featureID);
                    Logger.getLogger(PNGWriter.class.getName()).log(Level.SEVERE, null, ex);
                }
            }
        }

        File outputfile = new File(dsname + "_" + featureID + "_" + stratid + ".png");
        try {
            ImageIO.write(awtImage, "png", outputfile);
            return awtImage;
        } catch (IOException ex) {
            Logger.getLogger(PNGWriter.class.getName()).log(Level.SEVERE, null, ex);
        }
        return awtImage;
    }

}
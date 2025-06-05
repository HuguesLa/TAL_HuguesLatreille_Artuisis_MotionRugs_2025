package dbvis.motionrugs.gui;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Dimension;
import java.awt.Font;
import java.awt.Graphics;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Image;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.image.BufferedImage;
import javax.swing.BorderFactory;
import javax.swing.JButton;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.SwingUtilities;

/**
 * Panel displaying the Motion Rugs
 */
public class VisPanel extends JPanel {
    
    private BufferedImage img;
    private JPanel headerPanel;
    private JLabel titleLabel;
    private JButton deleteButton;
    private String datasetName;
    private String featureName;
    private String strategyName;
    
    /**
     * Creates a new Panel with the given image
     * @param img the image to display
     */
    public VisPanel(BufferedImage img) {
        this(img, null, null, null);
    }
    
    /**
     * Creates a new Panel with the given image and metadata
     * @param img the image to display
     * @param dataset the dataset name
     * @param feature the feature name
     * @param strategy the strategy name
     */
    public VisPanel(BufferedImage img, String dataset, String feature, String strategy) {
        this.img = img;
        this.datasetName = dataset;
        this.featureName = feature;
        this.strategyName = strategy;
        
        setLayout(new BorderLayout());
        
        // Create header panel with title and delete button
        headerPanel = new JPanel(new GridBagLayout());
        headerPanel.setBorder(BorderFactory.createMatteBorder(0, 0, 1, 0, Color.GRAY));
        headerPanel.setBackground(new Color(240, 240, 240));
        
        GridBagConstraints gbc = new GridBagConstraints();
        
        // Delete button
        deleteButton = new JButton("×");
        deleteButton.setToolTipText("Remove this visualization");
        deleteButton.setFont(new Font("Sans-Serif", Font.BOLD, 14));
        deleteButton.setMargin(new java.awt.Insets(0, 5, 0, 5));
        deleteButton.setFocusPainted(false);
        gbc.gridx = 0;
        gbc.gridy = 0;
        gbc.weightx = 0.0;
        gbc.fill = GridBagConstraints.NONE;
        gbc.anchor = GridBagConstraints.WEST;
        gbc.insets = new java.awt.Insets(2, 8, 2, 2);
        headerPanel.add(deleteButton, gbc);
        
        // Title label
        String titleText = feature != null ? feature : "Motion Rug";
        if (dataset != null) {
            titleText = dataset + " - " + titleText;
        }
        if (strategy != null) {
            titleText += " (" + strategy + ")";
        }
        
        titleLabel = new JLabel(titleText);
        titleLabel.setFont(new Font("Sans-Serif", Font.BOLD, 12));
        gbc.gridx = 1;
        gbc.weightx = 1.0;
        gbc.fill = GridBagConstraints.HORIZONTAL;
        gbc.insets = new java.awt.Insets(4, 8, 4, 4);
        headerPanel.add(titleLabel, gbc);
        
        deleteButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                JScrollPane scrollPane = (JScrollPane) SwingUtilities.getAncestorOfClass(JScrollPane.class, VisPanel.this);
                if (scrollPane != null) {
                    JPanel parentPanel = (JPanel) scrollPane.getParent();
                    if (parentPanel != null) {
                        parentPanel.remove(scrollPane);
                        parentPanel.revalidate();
                        parentPanel.repaint();
                    }
                }
            }
        });
        
        add(headerPanel, BorderLayout.NORTH);
        
        if (img != null) {
            // Set preferred size for scrolling
            int panelWidth = Math.max(img.getWidth(), 800);
            int panelHeight = img.getHeight() + headerPanel.getPreferredSize().height;
            setPreferredSize(new Dimension(panelWidth, panelHeight));
        }
    }
    
    /**
     * Updates the image displayed in the panel
     * @param newImg the new image to display
     */
    public void setImage(BufferedImage newImg) {
        this.img = newImg;
        if (newImg != null) {
            int panelWidth = Math.max(newImg.getWidth(), 800);
            int panelHeight = newImg.getHeight() + headerPanel.getPreferredSize().height;
            setPreferredSize(new Dimension(panelWidth, panelHeight));
        }
        revalidate();
        repaint();
    }
    
    /**
     * Updates the metadata displayed in the panel
     * @param dataset the dataset name
     * @param feature the feature name
     * @param strategy the strategy name
     */
    public void updateMetadata(String dataset, String feature, String strategy) {
        this.datasetName = dataset;
        this.featureName = feature;
        this.strategyName = strategy;
        
        String titleText = feature != null ? feature : "Motion Rug";
        if (dataset != null) {
            titleText = dataset + " - " + titleText;
        }
        if (strategy != null) {
            titleText += " (" + strategy + ")";
        }
        
        titleLabel.setText(titleText);
    }
    
    @Override
    protected void paintComponent(Graphics g) {
        super.paintComponent(g);
        if (img != null) {
            g.drawImage(img, 0, headerPanel.getHeight(), this);
        }
    }
    
    /**
     * Returns the image used by this panel
     * @return the image 
     */
    public Image getImage() {
        return img;
    }
}
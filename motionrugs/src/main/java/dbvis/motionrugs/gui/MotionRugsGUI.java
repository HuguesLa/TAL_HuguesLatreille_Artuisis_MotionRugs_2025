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

import dbvis.motionrugs.data.CSVDataLoader;
import dbvis.motionrugs.data.DataPoint;
import dbvis.motionrugs.data.DataSet;
import dbvis.motionrugs.data.SessionData;
import dbvis.motionrugs.strategies.HilbertCurveStrategy;
import dbvis.motionrugs.strategies.HilbertV2;
import dbvis.motionrugs.strategies.QuadTreeStrategy;
import dbvis.motionrugs.strategies.RTreeStrategy;
import dbvis.motionrugs.strategies.Strategy;
import dbvis.motionrugs.strategies.ZOrderCurveStrategy;

import java.awt.Dimension;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.image.BufferedImage;
import java.util.logging.Level;
import java.util.logging.Logger;
import javax.swing.BoxLayout;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.event.ChangeEvent;
import javax.swing.event.ChangeListener;

import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

import javax.swing.JScrollBar;
import javax.swing.SwingUtilities;

/**
 * MotionRugs main gui. Initializes processing of the rugs.
 * 
 * @author Juri Buchmüller, University of Konstanz <buchmueller@dbvis.inf.uni-konstanz.de>
 */
public class MotionRugsGUI extends javax.swing.JFrame {
    
    private DataSet curDataSet;
    private JPanel addPanel = new JPanel();
    
    // Ordering Strategies have to be instantiated here and added below where marked
    private Strategy pqrstrategy = new QuadTreeStrategy();
    private Strategy rtreestrategy = new RTreeStrategy();
    private Strategy zorderstrategy = new ZOrderCurveStrategy();
    private HilbertCurveStrategy hilbertcurvestrategy = new HilbertCurveStrategy();
    private HilbertV2 hilbertv2strategy = new HilbertV2();
    
    // Auto-refresh variables
    private Timer autoRefreshTimer;
    private double refreshIntervalSeconds = 1.0; 
    private boolean autoRefreshEnabled = false;
    private String currentSelectedDataset;
    private String currentSelectedFeature;
    private String currentSelectedStrategy;
    
    private javax.swing.JToggleButton jToggleAutoRefresh;
    private javax.swing.JSpinner jSpinnerInterval;
    private javax.swing.JLabel jLabelInterval;
    private javax.swing.JPanel jPanelRefresh;
    
    private javax.swing.JPanel jPanelScaleFactor;
    private javax.swing.JLabel jLabelScaleFactor;
    private javax.swing.JSpinner jSpinnerScaleFactor;
    
    private JScrollPane autoRefreshScrollPane;
    private VisPanel autoRefreshPanel;
    
    /**
     * Constructor initializing the datasets and strategies
     */
    public MotionRugsGUI(String[] datadir) {
        addPanel.setLayout(new BoxLayout(addPanel, BoxLayout.PAGE_AXIS));
        
        // List CSV
        List<String> csvFiles = CSVDataLoader.listCSVFiles(datadir);
        
        SessionData data = SessionData.getInstance();
        initComponents();
        initAutoRefreshComponents();
        initScaleFactorComponents();

        if (csvFiles.isEmpty()) {
            Logger.getLogger(MotionRugsGUI.class.getName()).log(Level.SEVERE, null, "NO DATASETS FOUND.");
            System.exit(-1);
        }
        
        // Populate dataset dropdown
        jComboBox4.removeAllItems();
        for (String fileName : csvFiles) {
            jComboBox4.addItem(fileName);
        }

        // Event listener for dataset selection
        jComboBox4.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                String selectedFile = jComboBox4.getItemAt(jComboBox4.getSelectedIndex());
                
                // Load dataset if not already loaded
                if (data.getDataset(selectedFile.replace(".csv", "")) == null) {
                    CSVDataLoader.loadSelectedFile(selectedFile);
                    System.out.println("Loaded file: " + selectedFile);
                }
                
                // Update feature dropdown
                curDataSet = data.getDataset(selectedFile.replace(".csv", ""));
                if (curDataSet != null) {
                    jComboBox5.removeAllItems();
                    for (String s : curDataSet.getFeatureList()) {
                        if (s.equals("frame") || s.equals("id") || s.equals("x") || s.equals("y")) {
                            continue;
                        }
                        jComboBox5.addItem(s);
                    }
                }
            }
        });

        jComboBox6.removeAllItems();
        
        // Adding Strategy to selection menu
        jComboBox6.addItem("Hilbert curve");
        jComboBox6.addItem("HilbertV2");
        jComboBox6.addItem("Point QuadTree");
        jComboBox6.addItem("R-Tree");
        jComboBox6.addItem("Z-Order");

        jButton2.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                currentSelectedDataset = jComboBox4.getItemAt(jComboBox4.getSelectedIndex());
                currentSelectedFeature = jComboBox5.getItemAt(jComboBox5.getSelectedIndex());
                currentSelectedStrategy = jComboBox6.getItemAt(jComboBox6.getSelectedIndex());
                
                // Ensure dataset is loaded
                if (data.getDataset(currentSelectedDataset.replace(".csv", "")) == null) {
                    CSVDataLoader.loadSelectedFile(currentSelectedDataset);
                }
                
                DataSet current = SessionData.getInstance().getDataset(currentSelectedDataset.replace(".csv", ""));
                BufferedImage bf = null;
                DataPoint[][] orderedpoints = null;

                // Order data based on selected strategy
                switch (currentSelectedStrategy) {
                    case "Point QuadTree":
                        orderedpoints = pqrstrategy.getOrderedValues(current.getBaseData());
                        break;
                    case "R-Tree":
                        orderedpoints = rtreestrategy.getOrderedValues(current.getBaseData());
                        break;
                    case "Hilbert curve":
                        hilbertcurvestrategy.setHilbertOrder(100);
                        orderedpoints = hilbertcurvestrategy.getOrderedValues(current.getBaseData());
                        break;
                    case "HilbertV2":
                        hilbertv2strategy.setHilbertOrder(100);
                        orderedpoints = hilbertv2strategy.getOrderedValues(current.getBaseData());
                        break;
                    case "Z-Order":
                        orderedpoints = zorderstrategy.getOrderedValues(current.getBaseData());
                        break;
                }
                
                // Create image from reordered data
                bf = PNGWriter.drawAndSaveRugs(orderedpoints, current.getMin(currentSelectedFeature), 
                        current.getMax(currentSelectedFeature), current.getDeciles(currentSelectedFeature), 
                        currentSelectedFeature, current.getName(), currentSelectedStrategy);
                System.out.println("DONE REORDERING"); 
                
                if (autoRefreshEnabled) {
                    // If auto-refresh is enabled, update or create auto-refresh panel
                    updateAutoRefreshPanel(bf, currentSelectedDataset.replace(".csv", ""), 
                            currentSelectedFeature, currentSelectedStrategy);
                } else {
                    // If auto-refresh is disabled, add static panel
                    repaintPanel(bf);
                }
            }
        });
        
        // Setup auto refresh timer
        setupAutoRefresh();
    }
    
    /**
     * Initialize auto refresh UI components
     */
    private void initAutoRefreshComponents() {
        jPanelRefresh = new javax.swing.JPanel();
        jToggleAutoRefresh = new javax.swing.JToggleButton("Auto Refresh");
        jLabelInterval = new javax.swing.JLabel("Interval (sec):");
        jSpinnerInterval = new javax.swing.JSpinner(new javax.swing.SpinnerNumberModel(1.0, 0.1, 300.0, 0.1));
        
        // Setup refresh panel layout
        javax.swing.GroupLayout jPanelRefreshLayout = new javax.swing.GroupLayout(jPanelRefresh);
        jPanelRefresh.setLayout(jPanelRefreshLayout);
        jPanelRefreshLayout.setHorizontalGroup(
            jPanelRefreshLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanelRefreshLayout.createSequentialGroup()
                .addContainerGap()
                .addComponent(jToggleAutoRefresh)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jLabelInterval)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jSpinnerInterval, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(50, Short.MAX_VALUE))
        );
        jPanelRefreshLayout.setVerticalGroup(
            jPanelRefreshLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, jPanelRefreshLayout.createSequentialGroup()
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .addGroup(jPanelRefreshLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(jToggleAutoRefresh)
                    .addComponent(jLabelInterval)
                    .addComponent(jSpinnerInterval, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))
                .addGap(14, 14, 14))
        );
        
        // Add panel to main UI
        jPanel1.add(jPanelRefresh);
        
        // Setup action listeners
        jToggleAutoRefresh.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                autoRefreshEnabled = jToggleAutoRefresh.isSelected();
                jSpinnerInterval.setEnabled(autoRefreshEnabled);
                if (autoRefreshEnabled) {
                    refreshIntervalSeconds = ((Number) jSpinnerInterval.getValue()).doubleValue();
                    // Restart timer with new interval
                    if (autoRefreshTimer != null) {
                        autoRefreshTimer.cancel();
                        autoRefreshTimer = null;
                    }
                    setupAutoRefresh();
                } else {
                    // Disable auto-refresh but keep the panel
                    disableAutoRefresh();
                }
            }
        });

        jSpinnerInterval.addChangeListener(new ChangeListener() {
            @Override
            public void stateChanged(ChangeEvent e) {
                if (autoRefreshEnabled) {
                    refreshIntervalSeconds = ((Number) jSpinnerInterval.getValue()).doubleValue();
                    // Restart timer with new interval
                    if (autoRefreshTimer != null) {
                        autoRefreshTimer.cancel();
                        autoRefreshTimer = null;
                    }
                    setupAutoRefresh();
                }
            }
        });
    }
    
    /**
     * Initialize scale factor UI components
     */
    private void initScaleFactorComponents() {
        jPanelScaleFactor = new javax.swing.JPanel();
        jLabelScaleFactor = new javax.swing.JLabel("Vertical Scale:");
        jSpinnerScaleFactor = new javax.swing.JSpinner(
                new javax.swing.SpinnerNumberModel(
                        PNGWriter.getVerticalScaleFactor(), // initial value
                        1,  // minimum
                        10, // maximum
                        1   // step
                )
        );
        
        // Setup scale factor panel layout
        javax.swing.GroupLayout jPanelScaleFactorLayout = new javax.swing.GroupLayout(jPanelScaleFactor);
        jPanelScaleFactor.setLayout(jPanelScaleFactorLayout);
        jPanelScaleFactorLayout.setHorizontalGroup(
            jPanelScaleFactorLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanelScaleFactorLayout.createSequentialGroup()
                .addContainerGap()
                .addComponent(jLabelScaleFactor)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jSpinnerScaleFactor, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(50, Short.MAX_VALUE))
        );
        jPanelScaleFactorLayout.setVerticalGroup(
            jPanelScaleFactorLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, jPanelScaleFactorLayout.createSequentialGroup()
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .addGroup(jPanelScaleFactorLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(jLabelScaleFactor)
                    .addComponent(jSpinnerScaleFactor, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))
                .addGap(14, 14, 14))
        );
        
        // Add panel to main UI
        jPanel1.add(jPanelScaleFactor);
        
        // Setup action listeners
        jSpinnerScaleFactor.addChangeListener(new ChangeListener() {
            @Override
            public void stateChanged(ChangeEvent e) {
                PNGWriter.setVerticalScaleFactor((Integer) jSpinnerScaleFactor.getValue());
            }
        });
    }
    
    /**
     * Setup the auto refresh timer
     */
    private void setupAutoRefresh() {
        if (autoRefreshTimer == null) {
            autoRefreshTimer = new Timer(true);
            long intervalMillis = Math.round(refreshIntervalSeconds * 1000);
            autoRefreshTimer.scheduleAtFixedRate(new TimerTask() {
                @Override
                public void run() {
                    if (autoRefreshEnabled && currentSelectedDataset != null && 
                        currentSelectedFeature != null && currentSelectedStrategy != null) {
                        java.awt.EventQueue.invokeLater(() -> {
                            refreshData();
                        });
                    }
                }
            }, intervalMillis, intervalMillis);
        }
    }
    
    /**
     * Disable auto-refresh but keep the panel
     */
    private void disableAutoRefresh() {
        autoRefreshEnabled = false;
        if (autoRefreshTimer != null) {
            autoRefreshTimer.cancel();
            autoRefreshTimer = null;
        }
        System.out.println("Auto-refresh disabled, panel retained as static.");
    }
    
    /**
     * Refresh data and update display
     */
    private void refreshData() {
        System.out.println("Refreshing data...");
        
        if (currentSelectedDataset == null) {
            System.out.println("No dataset selected, skipping refresh");
            return;
        }
        
        // Check if CSV file still exists
        String csvFileName = currentSelectedDataset;
        if (!csvFileName.toLowerCase().endsWith(".csv")) {
            csvFileName += ".csv";
        }
        
        String datasetName = csvFileName.replace(".csv", "");
        
        // Reload file
        CSVDataLoader.loadSelectedFile(csvFileName);
        System.out.println("Reloaded file: " + csvFileName);
        
        // Verify dataset loaded correctly
        SessionData data = SessionData.getInstance();
        DataSet current = data.getDataset(datasetName);
        
        if (current == null) {
            System.out.println("Failed to reload dataset: " + datasetName);
            return;
        }
        
        // Verify selected feature exists
        if (currentSelectedFeature == null || !current.getFeatureList().contains(currentSelectedFeature)) {
            System.out.println("Selected feature not available in dataset: " + currentSelectedFeature);
            return;
        }
        
        // Process with selected strategy
        BufferedImage bf = null;
        DataPoint[][] orderedpoints = null;
        
        // Use currently selected strategy
        switch (currentSelectedStrategy) {
            case "Point QuadTree":
                orderedpoints = pqrstrategy.getOrderedValues(current.getBaseData());
                break;
            case "R-Tree":
                orderedpoints = rtreestrategy.getOrderedValues(current.getBaseData());
                break;
            case "Hilbert curve":
                hilbertcurvestrategy.setHilbertOrder(100);
                orderedpoints = hilbertcurvestrategy.getOrderedValues(current.getBaseData());
                break;
            case "HilbertV2":
                hilbertv2strategy.setHilbertOrder(100);
                orderedpoints = hilbertv2strategy.getOrderedValues(current.getBaseData());
                break;
            case "Z-Order":
                orderedpoints = zorderstrategy.getOrderedValues(current.getBaseData());
                break;
        }
        
        // Generate new image
        bf = PNGWriter.drawAndSaveRugs(orderedpoints, current.getMin(currentSelectedFeature), 
                current.getMax(currentSelectedFeature), current.getDeciles(currentSelectedFeature), 
                currentSelectedFeature, current.getName(), currentSelectedStrategy);
        
        // Update display for auto-refresh
        updateAutoRefreshPanel(bf, datasetName, currentSelectedFeature, currentSelectedStrategy);
    }
    
    /**
     * Update panel with new image for auto-refresh, reusing existing panel
     * 
     * @param toAdd the Image to be displayed
     * @param datasetName the name of the dataset
     * @param featureName the name of the feature
     * @param strategyName the name of the strategy
     */
    private void updateAutoRefreshPanel(BufferedImage toAdd, String datasetName, String featureName, String strategyName) {
        if (autoRefreshScrollPane == null) {
            autoRefreshPanel = new VisPanel(toAdd, datasetName, featureName, strategyName);
            autoRefreshScrollPane = new JScrollPane(autoRefreshPanel, 
                    JScrollPane.VERTICAL_SCROLLBAR_NEVER, 
                    JScrollPane.HORIZONTAL_SCROLLBAR_ALWAYS);
            autoRefreshScrollPane.setPreferredSize(new Dimension(
                    Math.min(addPanel.getWidth(), 800),
                    Math.min(toAdd.getHeight() + 30, 500)));
            addPanel.add(autoRefreshScrollPane);
        } else {
            autoRefreshPanel.setImage(toAdd);
            autoRefreshPanel.updateMetadata(datasetName, featureName, strategyName);
        }
        
        // Refresh display
        addPanel.revalidate();
        addPanel.repaint();
        this.validate();
        
        // Auto-scroll to the right edge
        SwingUtilities.invokeLater(() -> {
            if (autoRefreshScrollPane != null) {
                JScrollBar scrollBar = autoRefreshScrollPane.getHorizontalScrollBar();
                scrollBar.setValue(scrollBar.getMaximum());
            }
        });
        
        System.out.println("Auto-refresh display updated and scrolled to newest data.");
    }
  
    /**
     * Repaints the Panel showing the visualizations by adding a new panel
     * 
     * @param toAdd the Image to be added to the VisPanel
     */
    private void repaintPanel(BufferedImage toAdd) {
        // Create a new VisPanel with the image and metadata
        VisPanel visPanel = new VisPanel(toAdd, 
                currentSelectedDataset.replace(".csv", ""), 
                currentSelectedFeature, 
                currentSelectedStrategy);
        
        // Create a new scroll pane with the modified VisPanel
        JScrollPane scrollPane = new JScrollPane(visPanel, 
                JScrollPane.VERTICAL_SCROLLBAR_NEVER, 
                JScrollPane.HORIZONTAL_SCROLLBAR_ALWAYS);
        
        // Add to panel
        addPanel.add(scrollPane);
        addPanel.validate();
        this.validate();
        
        // Auto-scroll to the right edge
        SwingUtilities.invokeLater(() -> {
            JScrollBar scrollBar = scrollPane.getHorizontalScrollBar();
            scrollBar.setValue(scrollBar.getMaximum());
        });
        
        System.out.println("Added and scrolled to right edge.");
    }

    /**
     * This method is called from within the constructor to initialize the form.
     * WARNING: Do NOT modify this code. The content of this method is always
     * regenerated by the Form Editor.
     */
    @SuppressWarnings("unchecked")
    // <editor-fold defaultstate="collapsed" desc="Generated Code">
    private void initComponents() {

        jPanel1 = new javax.swing.JPanel();
        jPanel6 = new javax.swing.JPanel();
        jLabel4 = new javax.swing.JLabel();
        jComboBox4 = new javax.swing.JComboBox<>();
        jPanel7 = new javax.swing.JPanel();
        jLabel5 = new javax.swing.JLabel();
        jComboBox5 = new javax.swing.JComboBox<>();
        jPanel8 = new javax.swing.JPanel();
        jLabel6 = new javax.swing.JLabel();
        jComboBox6 = new javax.swing.JComboBox<>();
        jPanel9 = new javax.swing.JPanel();
        jButton2 = new javax.swing.JButton();
        jScrollPane1 = new javax.swing.JScrollPane(addPanel);

        setDefaultCloseOperation(javax.swing.WindowConstants.EXIT_ON_CLOSE);
        setMinimumSize(new java.awt.Dimension(1024, 768));
        setSize(new java.awt.Dimension(1024, 768));

        jPanel1.setBorder(javax.swing.BorderFactory.createEtchedBorder());
        jPanel1.setLayout(new javax.swing.BoxLayout(jPanel1, javax.swing.BoxLayout.LINE_AXIS));

        jLabel4.setText("Dataset");

        jComboBox4.setModel(new javax.swing.DefaultComboBoxModel<>(new String[] { "Item 1", "Item 2", "Item 3", "Item 4" }));

        javax.swing.GroupLayout jPanel6Layout = new javax.swing.GroupLayout(jPanel6);
        jPanel6.setLayout(jPanel6Layout);
        jPanel6Layout.setHorizontalGroup(
            jPanel6Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel6Layout.createSequentialGroup()
                .addContainerGap()
                .addComponent(jLabel4)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jComboBox4, javax.swing.GroupLayout.PREFERRED_SIZE, 149, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(53, Short.MAX_VALUE))
        );
        jPanel6Layout.setVerticalGroup(
            jPanel6Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, jPanel6Layout.createSequentialGroup()
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .addGroup(jPanel6Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(jLabel4)
                    .addComponent(jComboBox4, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))
                .addGap(16, 16, 16))
        );

        jPanel1.add(jPanel6);

        jLabel5.setText("Feature");

        jComboBox5.setModel(new javax.swing.DefaultComboBoxModel<>(new String[] { "Item 1", "Item 2", "Item 3", "Item 4" }));

        javax.swing.GroupLayout jPanel7Layout = new javax.swing.GroupLayout(jPanel7);
        jPanel7.setLayout(jPanel7Layout);
        jPanel7Layout.setHorizontalGroup(
            jPanel7Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel7Layout.createSequentialGroup()
                .addContainerGap()
                .addComponent(jLabel5)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jComboBox5, javax.swing.GroupLayout.PREFERRED_SIZE, 149, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(53, Short.MAX_VALUE))
        );
        jPanel7Layout.setVerticalGroup(
            jPanel7Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, jPanel7Layout.createSequentialGroup()
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .addGroup(jPanel7Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(jLabel5)
                    .addComponent(jComboBox5, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))
                .addGap(16, 16, 16))
        );

        jPanel1.add(jPanel7);

        jLabel6.setText("Strategy");

        jComboBox6.setModel(new javax.swing.DefaultComboBoxModel<>(new String[] { "Item 1", "Item 2", "Item 3", "Item 4" }));

        javax.swing.GroupLayout jPanel8Layout = new javax.swing.GroupLayout(jPanel8);
        jPanel8.setLayout(jPanel8Layout);
        jPanel8Layout.setHorizontalGroup(
            jPanel8Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel8Layout.createSequentialGroup()
                .addContainerGap()
                .addComponent(jLabel6)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jComboBox6, javax.swing.GroupLayout.PREFERRED_SIZE, 149, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(53, Short.MAX_VALUE))
        );
        jPanel8Layout.setVerticalGroup(
            jPanel8Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, jPanel8Layout.createSequentialGroup()
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .addGroup(jPanel8Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(jLabel6)
                    .addComponent(jComboBox6, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))
                .addGap(16, 16, 16))
        );

        jPanel1.add(jPanel8);

        jButton2.setText("Add Rug");

        javax.swing.GroupLayout jPanel9Layout = new javax.swing.GroupLayout(jPanel9);
        jPanel9.setLayout(jPanel9Layout);
        jPanel9Layout.setHorizontalGroup(
            jPanel9Layout.createSequentialGroup()
            .addGroup(jPanel9Layout.createSequentialGroup()
                .addContainerGap()
                .addComponent(jButton2)
                .addContainerGap(169, Short.MAX_VALUE))
        );
        jPanel9Layout.setVerticalGroup(
            jPanel9Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, jPanel9Layout.createSequentialGroup()
                .addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .addComponent(jButton2)
                .addGap(14, 14, 14))
        );

        jPanel1.add(jPanel9);

        javax.swing.GroupLayout layout = new javax.swing.GroupLayout(getContentPane());
        getContentPane().setLayout(layout);
        layout.setHorizontalGroup(
            layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addComponent(jPanel1, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
            .addComponent(jScrollPane1)
        );
        layout.setVerticalGroup(
            layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(layout.createSequentialGroup()
                .addComponent(jPanel1, javax.swing.GroupLayout.PREFERRED_SIZE, 46, javax.swing.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 716, Short.MAX_VALUE))
        );

        pack();
    }

    /**
     * GUI Starter
     * 
     * @param args The first String determines the data directory containing the datasets to be processed. If not set, defaults to /data/*
     */
    public static void main(String args[]) {
        /* Set the Nimbus look and feel */
        try {
            for (javax.swing.UIManager.LookAndFeelInfo info : javax.swing.UIManager.getInstalledLookAndFeels()) {
                if ("Nimbus".equals(info.getName())) {
                    javax.swing.UIManager.setLookAndFeel(info.getClassName());
                    break;
                }
            }
        } catch (ClassNotFoundException | InstantiationException | IllegalAccessException | javax.swing.UnsupportedLookAndFeelException ex) {
            java.util.logging.Logger.getLogger(MotionRugsGUI.class.getName()).log(java.util.logging.Level.SEVERE, null, ex);
        }

        /* Create and display the form */
        java.awt.EventQueue.invokeLater(() -> new MotionRugsGUI(args).setVisible(true));
    }

    // Variables declaration
    private javax.swing.JButton jButton2;
    private javax.swing.JComboBox<String> jComboBox4;
    private javax.swing.JComboBox<String> jComboBox5;
    private javax.swing.JComboBox<String> jComboBox6;
    private javax.swing.JLabel jLabel4;
    private javax.swing.JLabel jLabel5;
    private javax.swing.JLabel jLabel6;
    private javax.swing.JPanel jPanel1;
    private javax.swing.JPanel jPanel6;
    private javax.swing.JPanel jPanel7;
    private javax.swing.JPanel jPanel8;
    private javax.swing.JPanel jPanel9;
    private javax.swing.JScrollPane jScrollPane1;
    // End of variables declaration
}
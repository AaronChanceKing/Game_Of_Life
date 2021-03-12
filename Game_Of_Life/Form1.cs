using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game_Of_Life
{
    public partial class Form1 : Form
    {
        //Variables
        #region Variables
        // The universe array
        bool[,] universe = new bool[10, 10];
        bool[,] scratchPad = new bool[10, 10];

        // Drawing colors
        Color gridColor = Color.Black;
        Color cellColor = Color.Gray;
        Color toggleColor = Color.Transparent;

        // The Timer class
        Timer timer = new Timer();
        int interval = 100;

        // Generation count
        int generations = 0;

        //Cells alive
        int aliveCells = 0;

        //Bool for toggling grid format
        bool finite = false;

        //Bool to toggle neighbor count on/off
        bool neighborCount = true;

        //Bool to toggle the HUD
        bool hud = true;

        //Seed for random
        int seed = 0;

        #endregion

        public Form1()
        {
            InitializeComponent();

            //Read Settings
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            interval = Properties.Settings.Default.Timer;
            finite = Properties.Settings.Default.GridStyle;
            hud = Properties.Settings.Default.HUD;
            universe = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];
            scratchPad = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];

            //Change Title
            this.Text = Properties.Resources.AppName;


            // Setup the timer
            timer.Interval = interval; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer stoped

            // Status strip timer interval
            toolStripStatusInterval.Text = "Timer Interval = " + interval.ToString();

            // Status strip Cell Size
            toolStripStatusCellSize.Text = "Universe Size = {" + universe.GetLength(0) + "}{" + universe.GetLength(1) + "}";
        }

        //Alive count
        private int AliveCountInt()
        {
            // Iterate through the universe in the y, top to bottom
            for (int yCell = 0; yCell < universe.GetLength(1); yCell++)
            {
                // Iterate through the universe in the x, left to right
                for (int xCell = 0; xCell < universe.GetLength(0); xCell++)
                {
                    if (universe[xCell, yCell] == true)
                    {
                        aliveCells++;
                    }
                }
            }
            return aliveCells;
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {
            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    //Checks if cell is alive in universe
                    if (universe[x, y] == true)
                    {
                        if (CountNeighbors(x, y) < 2)
                        {
                            //Under Population
                            scratchPad[x, y] = false;
                        }
                        else if (CountNeighbors(x, y) > 3)
                        {
                            //Over Population
                            scratchPad[x, y] = false;
                        }
                        else if (CountNeighbors(x, y) == 2 || CountNeighbors(x, y) == 3)
                        {
                            //Stays alive
                            scratchPad[x, y] = true;
                        }
                    }
                    //Checks if cell is dead in universe
                    if (universe[x, y] == false)
                    {
                        if (CountNeighbors(x, y) == 3)
                        {
                            //Birth of cell
                            scratchPad[x, y] = true;
                        }
                    }
                }
            }
            //Swap universe with scrach pad
            bool[,] hold = universe;
            universe = scratchPad;
            scratchPad = hold;

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    //empty the scrach pad
                    scratchPad[x, y] = false;
                    //Empty alive cells
                    aliveCells = 0;
                }
            }

            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();

            // Increment generation count
            generations++;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        //Paints the graphics panel
        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            float cellWidth = (float)graphicsPanel1.ClientSize.Width / (float)universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            float cellHeight = (float)graphicsPanel1.ClientSize.Height / (float)universe.GetLength(1);

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);

            //Semi-transparent color for HUD
            Color custom = Color.FromArgb(175, Color.Red);
            //A brush for the HUD string
            Brush hudBrush = new SolidBrush(custom);

            //Set up the neighbor count
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            //Get Font size
            float area = cellHeight / 3;
            Font font = new Font("Arial", area);

            // A rectangle for HUD
            Rectangle hudRect = Rectangle.Empty;
            hudRect.X = graphicsPanel1.ClientSize.Width % 10;
            hudRect.Y = graphicsPanel1.ClientSize.Height / 2;
            hudRect.Width = graphicsPanel1.ClientSize.Width / 2;
            hudRect.Height = graphicsPanel1.ClientSize.Height / 2;

            //Set up the stringFormat
            StringFormat hudFormat = new StringFormat();
            hudFormat.Alignment = StringAlignment.Near;
            hudFormat.LineAlignment = StringAlignment.Near;

            //Get Font size
            int fontSize = graphicsPanel1.ClientSize.Height / 20;
            Font hudFont = new Font("Arial", fontSize);

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    RectangleF cellRect = RectangleF.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    //Get count
                    int neighbors = CountNeighbors(x, y);

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                     // Outline the cell with a pen
                    e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    
                    //Draw neibhor count
                    if (neighborCount == true)
                    {
                        //Draw cell count
                        if (neighbors > 0)
                        {
                            if (neighbors == 2 || neighbors == 3)
                            {
                                //Green for 2 or 3 cells
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Green, cellRect, stringFormat);
                            }
                            else
                            {
                                //Red for any other amount of cells
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Red, cellRect, stringFormat);
                            }

                        }
                    }
                }
            }

            //Draw HUD
            if (hud == true)
            {
                if (finite == true)
                {
                    e.Graphics.DrawString("\n\nGenerations = " + generations.ToString() + "\nAlive = " + AliveCountInt().ToString() + "\nBoundary Type = Finite" + "\nGrid SIze = (" + universe.GetLength(0) + ", " + universe.GetLength(1) + ")", hudFont, hudBrush, hudRect, hudFormat);
                }
                else
                {
                    e.Graphics.DrawString("\n\nGenerations = " + generations.ToString() + "\nAlive = " + AliveCountInt().ToString() + "\nBoundary Type = Tordial" + "\nGrid SIze = (" + universe.GetLength(0) + ", " + universe.GetLength(1) + ")", hudFont, hudBrush, hudRect, hudFormat);
                }
                aliveCells = 0;
            }

            //Draw alive count to strip
            toolStripStatusLabelAlive.Text = "Alive = " + AliveCountInt().ToString();
            //Reset the alive count for next generation
            aliveCells = 0;

            // Cleaning up pens and brushes
            hudBrush.Dispose();
            gridPen.Dispose();
            cellBrush.Dispose();
        }

        //Activate or deactivate cell on left mouse click
        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                float cellWidth = (float)graphicsPanel1.ClientSize.Width / (float)universe.GetLength(0);
                float cellHeight = (float)graphicsPanel1.ClientSize.Height / (float)universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                float x = e.X / cellWidth;
                // CELL Y = MOUSE Y / CELL HEIGHT
                float y = e.Y / cellHeight;

                // Toggle the cell's state
                universe[(int)x, (int)y] = !universe[(int)x, (int)y];

                
                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }

        //Counting Neighbors 
        #region Counting Neighbors
        //Finite
        private int CountNeighbors(int x, int y)
        {
            if (finite == true)
            {
                int count = 0;
                int xLen = universe.GetLength(0);
                int yLen = universe.GetLength(1);

                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        int xCheck = x + xOffset;
                        int yCheck = y + yOffset;

                        //if xOffset and yOffset are both equal to 0 continue
                        if (xOffset == 0 && yOffset == 0)
                        {
                            continue;
                        }
                        if (xCheck < 0)
                        {
                            continue;
                        }
                        if (yCheck < 0)
                        {
                            continue;
                        }
                        if (xCheck >= xLen)
                        {
                            continue;
                        }
                        if (yCheck >= yLen)
                        {
                            continue;
                        }
                        //Increase the neighbor count
                        if (universe[xCheck, yCheck] == true) count++;
                    }
                }

                return count;
            }
            else
            {
                return CountNeghborsToroidal(x, y);
            }
        }
        //Toroidal
        private int CountNeghborsToroidal(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);

            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }
                    if (xCheck < 0)
                    {
                        xCheck = (xLen - 1);
                    }
                    if (yCheck < 0)
                    {
                        yCheck = (yLen - 1);
                    }
                    if (xCheck >= xLen)
                    {
                        xCheck = 0;
                    }
                    if (yCheck >= yLen)
                    {
                        yCheck = 0;
                    }
                    //Increase the neighbor count
                    if (universe[xCheck, yCheck] == true) count++;
                }
            }

            return count;
        }

        #endregion

        //Menu options
        #region Options
        //BUG GAME
        private static void ThreadProc()
        {
            Application.Run(new Bug());
        }
        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            //Open Bug panel and close Form1 panel
            System.Threading.Thread bug = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadProc));
            bug.Start();
            this.Close();
        }

        //New menu button
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                }
            }
            //Resets generation to 0
            generations = 0;

            //Stops timer
            timer.Enabled = false;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();

            //Enable Start
            toolStripButtonStart.Enabled = true;
            //Disable Pause
            toolStripButtonPause.Enabled = false;

            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            newToolStripMenuItem_Click(sender, e);
        }

        //Start menu button
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //Enable the timer
            timer.Enabled = true;

            //Enable Pause
            toolStripButtonPause.Enabled = true;
            //Disable Start
            toolStripButtonStart.Enabled = false;
            //Disable Step
            toolStripButtonNext.Enabled = false;

            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }

        //Pause menu button
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //Pause the timeer
            timer.Enabled = false;

            //Enable Start
            toolStripButtonStart.Enabled = true;
            //Disable Pause
            toolStripButtonPause.Enabled = false;
            //Enable Step
            toolStripButtonNext.Enabled = true;
        }

        //Step menu button
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }

        //Exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Will end the program if selected
            Application.Exit();
        }

        //Settings Closed
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Read in settings
            Properties.Settings.Default.BackColor = graphicsPanel1.BackColor;
            Properties.Settings.Default.GridColor = gridColor;
            Properties.Settings.Default.CellColor = cellColor;
            Properties.Settings.Default.Timer = interval;
            Properties.Settings.Default.GridStyle = finite;
            Properties.Settings.Default.HUD = hud;
            Properties.Settings.Default.GridWidth = universe.GetLength(0);
            Properties.Settings.Default.GridHeight = universe.GetLength(1);


            //Set settings
            Properties.Settings.Default.Save();
        }
        //Reset Settings to default
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();

            //Read Settings
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            interval = Properties.Settings.Default.Timer;
            finite = Properties.Settings.Default.GridStyle;
            hud = Properties.Settings.Default.HUD;
            universe = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];
            scratchPad = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];

            // Status strip timer interval
            toolStripStatusInterval.Text = "Timer Interval = " + interval.ToString();

            // Status strip Cell Size
            toolStripStatusCellSize.Text = "Universe Size = {" + universe.GetLength(0) + "}{" + universe.GetLength(1) + "}";

            //Repaint panel
            graphicsPanel1.Invalidate();
        }
        //Reload Settings to last 'saved' setting
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            //Read Settings
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            interval = Properties.Settings.Default.Timer;
            finite = Properties.Settings.Default.GridStyle;
            hud = Properties.Settings.Default.HUD;
            universe = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];
            scratchPad = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];

            // Status strip timer interval
            toolStripStatusInterval.Text = "Timer Interval = " + interval.ToString();

            // Status strip Cell Size
            toolStripStatusCellSize.Text = "Universe Size = {" + universe.GetLength(0) + "}{" + universe.GetLength(1) + "}";

            //Repaint panel
            graphicsPanel1.Invalidate();
        }

        //Options Button
        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Options options = new Options();

            //Set default options to Modial dialog
            options.Timer = interval;
            options.CellWidth = universe.GetLength(0);
            options.CellHeight = universe.GetLength(1);

            //Check if ok is selected
            if (DialogResult.OK == options.ShowDialog())
            {
                //Change the variables in form1
                interval = options.Timer;
                universe = new bool[options.CellWidth, options.CellHeight];
                scratchPad = new bool[options.CellWidth, options.CellHeight];

                graphicsPanel1.Invalidate();

                //Update status strip
                // Status strip timer interval
                toolStripStatusInterval.Text = "Timer Interval = " + interval.ToString();

                // Status strip Cell Size
                toolStripStatusCellSize.Text = "Universe Size = {" + universe.GetLength(0) + "}{" + universe.GetLength(1) + "}";
            }
        }

        //Save Button
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            //New save Modal Dialog
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "All Files|*.*|Cells|*.cells";
            save.FilterIndex = 2; save.DefaultExt = "cells";

            //Checks to see if ok is selected
            if (DialogResult.OK == save.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(save.FileName);

                //Iterate thought the universe
                for(int y = 0; y < universe.GetLength(1); y++)
                {
                    //Create a string to represent the current row
                    String currentRow = string.Empty;
                    for(int x = 0; x < universe.GetLength(0); x++)
                    {
                        if (universe[x, y] == true)
                        {
                            //append 'O' for true
                            currentRow += "O";
                        }
                        else
                        {
                            //append '.' for false
                            currentRow += ".";
                        }
                    }
                    //Write to file using WriteLine
                    writer.WriteLine(currentRow);
                }
                //Close Writer
                writer.Close();

            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveToolStripButton_Click(sender, e);
        }

        //Load Button
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            //New open Modal Dialog
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "All Files|*.*|Cells|*.cells";
            open.FilterIndex = 2;

            //Checks to see if ok is selected
            if(DialogResult.OK == open.ShowDialog())
            {
                    StreamReader reader = new StreamReader(open.FileName);
                    //Create variables to calculate width and height of data
                    int maxWidth = 0;
                    int maxHeight = 0;

                    //Iterate though file to get its size
                    while (!reader.EndOfStream)
                    {
                        string row = reader.ReadLine();
                        if (row.StartsWith("!"))
                        {
                            continue;
                        }
                        else
                        {
                            maxHeight++;
                        }
                        maxWidth = row.Length;
                    }
                    //Resize universe/Scrachpad
                    universe = new bool[maxWidth, maxHeight];
                    scratchPad = new bool[maxWidth, maxHeight];

                    //Reset file pointer back to beggining of file
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);

                //Iterate though file
                while (!reader.EndOfStream)
                {
                    for (int y = 0; y < universe.GetLength(1); y++)
                    {
                        string row = reader.ReadLine();

                        if (row.StartsWith("!"))
                        {
                            continue;
                        }
                        else
                        {
                            //Iterate through row of cells
                            for (int x = 0; x < row.Length; x++)
                            {

                                if (row[x] == 'O')
                                {
                                    //Set cell to alive
                                    universe[x, y] = true;
                                }
                                else
                                {
                                    //Set cell to dead
                                    universe[x, y] = false;
                                }
                            }
                        }
                    }
                }
                //Close file
                reader.Close();
            }

            // Status strip timer interval
            toolStripStatusInterval.Text = "Timer Interval = " + interval.ToString();

            // Status strip Cell Size
            toolStripStatusCellSize.Text = "Universe Size = {" + universe.GetLength(0) + "}{" + universe.GetLength(1) + "}";

            //Redraw panel
            graphicsPanel1.Invalidate();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openToolStripButton_Click(sender, e);
        }

         //Random options
        #region Random
        //Random 1/3
        private void randomToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //Initilize Random class
            Random rng = new Random();

            //Clear the current universe
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                }
            }

            //Set cells to alive based on rng         
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int alive = rng.Next(0, 3);
                    if (alive == 0)
                    {
                        universe[x, y] = true;
                    }
                }
            }
            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }

        //Random by time
        private void randomByTimeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //Initilize Random class using Date Time
            int seed = (int)DateTime.Now.Ticks;
            Random rng = new Random(seed);

            //Clear the current universe
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                }
            }

            //Set cells to alive based on rng         
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int alive = rng.Next(0, seed);
                    if ((seed / 2) < alive)
                    {
                        universe[x, y] = true;
                    }
                }
            }
            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }

        //Random by seed
        private void randomBySeedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //New seed modial dialog box
            Seed seedModial = new Seed();

            //Display current seed
            seedModial.SeedInput = seed;

            //Check if ok is selected
            if (DialogResult.OK == seedModial.ShowDialog())
            {
                 seed = seedModial.SeedInput;
            }

            //Initilize Random class using seed
            Random rng = new Random(seed);

            //Clear the current universe
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                }
            }

            //Set cells to alive based on rng         
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int alive = rng.Next(0, seed);
                    if ((seed / 2) < alive)
                    {
                        universe[x, y] = true;
                    }
                }
            }
            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }
        #endregion

        //Color Changer
        #region Colors

        //Grid
        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog color = new ColorDialog();
            //Display current color
            color.Color = gridColor;

            //Check if ok is selected
            if (DialogResult.OK == color.ShowDialog())
            {
                gridColor = color.Color;
            }

            //Tell window to redraw
            graphicsPanel1.Invalidate();
        }
        //Cell
        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog color = new ColorDialog();
            //Display current color
            color.Color = cellColor;

            //Check if ok is selected
            if (DialogResult.OK == color.ShowDialog())
            {
                cellColor = color.Color;
            }

            //Tell window to redraw
            graphicsPanel1.Invalidate();
        }
        //Back Panel
        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog color = new ColorDialog();
            //Display current color
            color.Color = graphicsPanel1.BackColor;

            //Check if ok is selected
            if (DialogResult.OK == color.ShowDialog())
            {
                graphicsPanel1.BackColor = color.Color;
            }

            //Tell window to redraw
            graphicsPanel1.Invalidate();
        }
        #endregion

        //Toggle options
        #region Toggles

        //Toggle Grid
        private void toggleGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Place holder color
            Color hold;

            //Swap grid color and toggle color
            hold = gridColor;
            gridColor = toggleColor;
            toggleColor = hold;


            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();

        }
        //Toggle neighbor count
        private void toggleNeighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(neighborCount == true)
            {
                neighborCount = false;
            }
            else
            {
                neighborCount = true;
            }

            graphicsPanel1.Invalidate();
        }
        //Grid Style toggle
        private void finiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Finite grid style
            finite = true;
            //Uncheck
            tordialToolStripMenuItem.Checked = false;
            //Check box
            finiteToolStripMenuItem.Checked = true;

            //Repaint panel
            graphicsPanel1.Invalidate();
        }
        private void tordialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Tordial grid style
            finite = false;
            //UnCheck
            finiteToolStripMenuItem.Checked = false;
            //Check box
            tordialToolStripMenuItem.Checked = true;

            //Repaint panel
            graphicsPanel1.Invalidate();

        }
        //Toggle HUD
        private void toggleHUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hud == true)
            {
                hud = false;
            }
            else
            {
                hud = true;
            }

            graphicsPanel1.Invalidate();
        }

        #endregion

        //Contex menu
        #region contex

        //Grid Color
        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorToolStripMenuItem_Click(sender, e);
        }
        //Cell Color
        private void cellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cellColorToolStripMenuItem_Click(sender, e);
        }
        //BackGround Color
        private void backGroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            backColorToolStripMenuItem_Click(sender, e);
        }

        //Save
        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveToolStripButton_Click(sender, e);
        }
        //Load
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openToolStripButton_Click(sender, e);
        }

        //Random 1/3
        private void randomToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            randomToolStripMenuItem1_Click(sender, e);
        }
        //--------------------------------
        //Random Time                   //
        //To-DO                         //
        //--------------------------------
        private void randomByTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        //--------------------------------
        //Random Seed                   //
        //To-DO                         //
        //--------------------------------
        private void randomBySeedToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #endregion

    }
}

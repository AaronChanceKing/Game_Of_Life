using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game_Of_Life
{
    public partial class Bug : Form
    {
        //Variables
        #region Variables

        // The universe array
        bool[,] bug = new bool[10, 10];
        bool[,] scrachPad = new bool[10, 10];
        bool[,] universe = new bool[10, 10];

        // Drawing colors
        Color bugColor = Color.Red;
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

        //Bool to toggle the HUD
        bool hud = true;

        //Bug movement
        enum move { North = 1, East = 2, South = 3, West = 4 };
        move direction = move.North;

        #endregion

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

        //Constructor
        public Bug()
        {
            InitializeComponent();

            //Read Settings
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            bugColor = Properties.Settings.Default.BugColor;
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            interval = Properties.Settings.Default.Timer;
            hud = Properties.Settings.Default.HUD;
            scrachPad = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];
            bug = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];
            universe = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];

            // Setup the timer
            timer.Interval = interval; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer stoped
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {
            // Iterate through the bug in the y, top to bottom
            for (int y = 0; y < bug.GetLength(1); y++)
            {
                // Iterate through the bug in the x, left to right
                for (int x = 0; x < bug.GetLength(0); x++)
                {
                    try
                    {
                        //If bug is in an alive cell
                        if (universe[x, y] == true && bug[x, y] == true)
                        {
                            //Turn off cell
                            universe[x, y] = false;

                            //Change direction 90 degrees and move forward one space
                            //Temporarally draw the new location of the bug to the scratchpad
                            switch (direction)
                            {
                                case move.North:
                                    direction = move.East;
                                    scrachPad[x + 1, y] = true;
                                    break;
                                case move.East:
                                    direction = move.South;
                                    scrachPad[x, y - 1] = true;
                                    break;
                                case move.South:
                                    direction = move.West;
                                    scrachPad[x - 1, y] = true;
                                    break;
                                case move.West:
                                    direction = move.North;
                                    scrachPad[x, y + 1] = true;
                                    break;

                            }
                        }
                        //If bug is in a dead cell
                        else if (universe[x, y] == false && bug[x, y] == true)
                        {
                            //Turn on cell
                            universe[x, y] = true;

                            //Change direction 90 degrees and move forward one space
                            //Temporarally draw the new location of the bug to the scratchpad
                            switch (direction)
                            {
                                case move.North:
                                    direction = move.West;
                                    scrachPad[x - 1, y] = true;
                                    break;
                                case move.West:
                                    direction = move.South;
                                    scrachPad[x, y - 1] = true;
                                    break;
                                case move.South:
                                    direction = move.East;
                                    scrachPad[x + 1, y] = true;
                                    break;
                                case move.East:
                                    direction = move.North;
                                    scrachPad[x, y + 1] = true;
                                    break;
                            }
                        }
                        //Remove old location for bug
                        bug[x, y] = false;
                    }
                    //Make sure bug isn't going out of the universe bounds
                    catch
                    {
                        //Stop Timer
                        timer.Stop();
                        //Display error message
                        Error error = new Error();
                        error.ShowDialog();

                        //Reset play button
                        toolStripButton1.Enabled = true;
                        //Reset pause button
                        toolStripButton2.Enabled = false;
                        //Reset step button
                        toolStripButton3.Enabled = true;
                    }
                }
            }
            //Swap bug and scratch pad
            bool[,] hold = bug;
            bug = scrachPad;
            scrachPad = hold;

            //Clear scrach pad
            // Iterate through the bug in the y, top to bottom
            for (int y = 0; y < bug.GetLength(1); y++)
            {
                // Iterate through the bug in the x, left to right
                for (int x = 0; x < bug.GetLength(0); x++)
                {
                    scrachPad[x, y] = false;
                }
            }

            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();

            // Increment generation count
            generations++;

        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        //Paint graphics panel
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
            Brush bugBrush = new SolidBrush(bugColor);


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

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    // Outline the cell with a pen
                    e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

                }
            }

            // Iterate through the bug in the y, top to bottom
            for (int y = 0; y < bug.GetLength(1); y++)
            {
                // Iterate through the bug in the x, left to right
                for (int x = 0; x < bug.GetLength(0); x++)
                {
                    // A rectangle to represent bug
                    RectangleF bugRect = RectangleF.Empty;
                    bugRect.X = x * cellWidth;
                    bugRect.Y = y * cellHeight;
                    bugRect.Width = cellWidth;
                    bugRect.Height = cellHeight;

                    //Fill cell if bug is alive
                    if (bug[x, y] == true)
                    {
                        e.Graphics.FillRectangle(bugBrush, bugRect);
                    }
                }
            }

            //Draw HUD
            if (hud == true)
            {
                e.Graphics.DrawString("\n\n\nGenerations = " + generations.ToString() + "\nAlive = " + AliveCountInt().ToString(), hudFont, Brushes.Red, hudRect, hudFormat);

                aliveCells = 0;
            }
            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
            bugBrush.Dispose();
        }

        //Activate or deactivate cell on left mouse click
        private void graphicsPanel1_MouseClick_1(object sender, MouseEventArgs e)
        {
            //Clears the bug from old array to allow only one bug at a time
            // Iterate through the bug in the y, top to bottom
            for (int y = 0; y < bug.GetLength(1); y++)
            {
                // Iterate through the bug in the x, left to right
                for (int x = 0; x < bug.GetLength(0); x++)
                {
                    bug[x, y] = false;
                }
            }

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
                bug[(int)x, (int)y] = !bug[(int)x, (int)y];


                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }

        //Options
        #region Options

        //Play button
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //Enable the timer
            timer.Enabled = true;

            //Enable Pause
            toolStripButton2.Enabled = true;
            //Disable Start
            toolStripButton1.Enabled = false;
            //Disable Step
            toolStripButton3.Enabled = false;

            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }

        //Pause button
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //Pause the timeer
            timer.Enabled = false;

            //Enable Start
            toolStripButton1.Enabled = true;
            //Disable Pause
            toolStripButton2.Enabled = false;
            //Enable Step
            toolStripButton3.Enabled = true;
        }

        //Next button
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }

        //Back to Conway
        private static void ThreadProc()
        {
            //Start a new form1
            Application.Run(new Form1());
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            //Open Form1 panel and close Bug panel
            System.Threading.Thread form1 = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadProc));
            form1.Start();
            this.Close();
        }

        //Exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Will end the program if selected
            Application.Exit();
        }

        //Clear
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                    bug[x, y] = false;
                }
            }
            //Resets generation to 0
            generations = 0;

            //Stops timer
            timer.Enabled = false;

            //Enable Start
            toolStripButton1.Enabled = true;
            //Disable Pause
            toolStripButton2.Enabled = false;
            //Enable step
            toolStripButton3.Enabled = true;

            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newToolStripButton_Click(sender, e);
        }

        //Settings Closed
        private void Bug_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Read in settings
            Properties.Settings.Default.BackColor = graphicsPanel1.BackColor;
            Properties.Settings.Default.BugColor = bugColor;
            Properties.Settings.Default.GridColor = gridColor;
            Properties.Settings.Default.CellColor = cellColor;
            Properties.Settings.Default.Timer = interval;
            Properties.Settings.Default.HUD = hud;
            Properties.Settings.Default.GridWidth = universe.GetLength(0);
            Properties.Settings.Default.GridHeight = universe.GetLength(1);

            //Set settings
            Properties.Settings.Default.Save();
        }
        //Reset
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();

            //Read Settings
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            bugColor = Properties.Settings.Default.BugColor;
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            interval = Properties.Settings.Default.Timer;
            hud = Properties.Settings.Default.HUD;
            scrachPad = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];
            universe = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];
            bug = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];


            //Repaint panel
            graphicsPanel1.Invalidate();
        }
        //Reload
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            //Read Settings
            graphicsPanel1.BackColor = Properties.Settings.Default.BackColor;
            bugColor = Properties.Settings.Default.BugColor;
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            interval = Properties.Settings.Default.Timer;
            hud = Properties.Settings.Default.HUD;
            scrachPad = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];
            universe = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];
            bug = new bool[(Properties.Settings.Default.GridWidth), (Properties.Settings.Default.GridHeight)];


            //Repaint panel
            graphicsPanel1.Invalidate();
        }

        //Toggle HUD
        private void toggleHUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Checks current statis of HUD
            if (hud == true)
            {
                //Turns HUD off
                hud = false;
                //unchecks the HUD toolStrip
                toggleHUDToolStripMenuItem.Checked = false;
            }
            else
            {
                //Turns on HUD
                hud = true;
                //Checks the HUD toolStrip
                toggleHUDToolStripMenuItem.Checked = true;
            }

            //Tells the window the re-paint
            graphicsPanel1.Invalidate();
        }

        //Toggle Grid
        private void toggleGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Place holder color
            Color hold;

            //Checks if toolStrip is checked
            if (gridColor == Color.Transparent)
            {
                toggleGridToolStripMenuItem.Checked = false;
            }
            else
            {
                toggleGridToolStripMenuItem.Checked = true;
            }

            //Swap grid color and toggle color
            hold = gridColor;
            gridColor = toggleColor;
            toggleColor = hold;


            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }

        //Options
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
                bug = new bool[options.CellWidth, options.CellHeight];
                universe = new bool[options.CellWidth, options.CellHeight];
                scrachPad = new bool[options.CellWidth, options.CellHeight];
            }
            //Tells the windown the re-Paint
            graphicsPanel1.Invalidate();
        }

        //Colors
        #region Colors
        //Grid Color
        private void gridColorToolStripMenuItem_Click(object sender, EventArgs e)
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
        //Cell Color
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
        //Back Color
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
        //Bug Color
        private void bugColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog color = new ColorDialog();
            //Display current color
            color.Color = bugColor;

            //Check if ok is selected
            if (DialogResult.OK == color.ShowDialog())
            {
                bugColor = color.Color;
            }

            //Tell window to redraw
            graphicsPanel1.Invalidate();
        }

        #endregion

        //Random
        //TODO
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
                    int alive = rng.Next(0, 2); //Generates # 0 - 2
                    if (alive == 0)
                    {
                        //If # is 0 turns that cell on
                        universe[x, y] = true;
                    }
                }
            }
            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }
        //Random 1/5
        private void randomToolStripMenuItem2_Click(object sender, EventArgs e)
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
                    int alive = rng.Next(0, 4); //Generates # 0 - 4
                    if (alive == 0)
                    {
                        //If # is 0 turns that cell on
                        universe[x, y] = true;
                    }
                }
            }
            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }
        //Random 1/10
        private void randomToolStripMenuItem3_Click(object sender, EventArgs e)
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
                    int alive = rng.Next(0, 9); //Generates # 0 - 9
                    if (alive == 0)
                    {
                        //If # is 0 turns that cell on
                        universe[x, y] = true;
                    }
                }
            }
            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }
        #endregion

        #endregion


    }
}


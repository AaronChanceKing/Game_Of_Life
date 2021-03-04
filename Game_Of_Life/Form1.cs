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
    public partial class Form1 : Form
    {
        // The universe array
        bool[,] universe = new bool[10, 10];
        bool[,] scratchPad = new bool[10, 10];

        // Drawing colors
        Color gridColor = Color.Black;
        Color cellColor = Color.Gray;
        Color toggleColor = Color.Transparent;

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

        //Bool for toggling grid format
        bool finite = true;

        //Bool to toggle neighbor count on/off
        bool neighborCount = true;

        public Form1()
        {
            InitializeComponent();

            //Change Title
            this.Text = Properties.Resources.AppName;

            // Setup the timer
            timer.Interval = 100; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer stoped
        }

        //Counting Neighbors Finite
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

        //Counting Neighbors Toroidal
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

            //Set up the neighbor count
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            //Get Font size
            float area = cellHeight / 3;
            Font font = new Font("Arial", area);

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

                    if (neighborCount == true)
                    {
                        //Draw cell count
                        if (neighbors > 0)
                        {
                            if (neighbors == 1)
                            {
                                //Black for 1 nearby cell
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Black, cellRect, stringFormat);
                            }
                            else if (neighbors == 2)
                            {
                                //Orange for 2 nearby cell
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.DarkOrange, cellRect, stringFormat);
                            }
                            else
                            {
                                //Red for more than 2 nearby cells
                                e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Firebrick, cellRect, stringFormat);
                            }

                        }
                    }
                }
            }

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
        }

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

        //Menu options
        #region Options
        //New menu pull down
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
            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }
        //New button
        private void newToolStripButton_Click(object sender, EventArgs e)
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
            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }
        //Start menu button
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //Enable the timer
            timer.Enabled = true;

            // Tell Windows you need to repaint
            graphicsPanel1.Invalidate();
        }
        //Pause menu button
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //Pause the timeer
            timer.Enabled = false;
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
        //Grid Style toggle
        private void finiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Finite grid style
            finite = true;
        }
        private void tordialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Tordial grid style
            finite = false;
        }

        //Random options
        #region Random
        //Random Region
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
        #endregion

        #endregion

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            //New save Modal Dialog
            SaveFileDialog save = new SaveFileDialog();

            //Checks to see if ok is selected
            if(DialogResult.OK == save.ShowDialog())
            {
                //TODO
            }
        }
    }
}

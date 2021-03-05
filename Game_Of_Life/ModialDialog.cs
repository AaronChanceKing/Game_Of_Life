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
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();

            //Change the title
            this.Text = Properties.Resources.Options;
        }

        //Timer interval prop
        public int Timer { get { return (int)numericUpDownTimer.Value; } set { numericUpDownTimer.Value = value; } }
        //Cell Width prop
        public int CellWidth { get { return (int)numericUpDownWidth.Value; } set { numericUpDownWidth.Value = value; } }
        //Cell Height prop
        public int CellHeight { get { return (int)numericUpDownHeight.Value; } set { numericUpDownHeight.Value = value; } }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notebook
{
    public partial class FormPhoneNumber : Form
    {
        public FormPhoneNumber()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (maskedTextBox.MaskCompleted)
            {
                PhoneNumber = maskedTextBox.Text;
            }
            
        }

        //для обмена номером телефона между двумя формами
        public string PhoneNumber
        {
            get => PhoneNumber = maskedTextBox.Text;
            set => maskedTextBox.Text = value;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox.Checked)
            {
                maskedTextBox.Mask = "+7(999) 000-00-00";
            }
            else
                maskedTextBox.Mask = "";

            buttonOK.Enabled = maskedTextBox.MaskCompleted;

        }

        private void maskedTextBox_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = maskedTextBox.MaskCompleted;
        }

        private void FormPhoneNumber_Load(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


/*
 * Text Editor -> 
 * 1. Array or array of arrays.
 * 2. Tree
 * 3. ---
 * the ids in sorted order
 * a - 1 , a - 2
 * Do binary search to get the position of insertion.
 * if we keep in a 1d array
 * before which character and after which charater.
 */

/*
 * keydown event -> we get the length of the text which is there in the editor
 */

namespace CollabText.Editor
{
    public partial class Form1 : Form 
    {
        CRDT cRDT = new CRDT();
        public Form1() 
        {
            InitializeComponent();
            textBox1.WordWrap = false;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var insertPosition = textBox1.SelectionStart-1;
            var position = textBox1.GetPositionFromCharIndex(insertPosition);

            var line = textBox1.GetLineFromCharIndex(insertPosition);
            var idx = textBox1.GetCharIndexFromPosition(position);
            char ch = textBox1.Text[idx];
            
            Console.Write(ch + " ");
            Console.Write(idx + " ");
            Console.Write(line + " ");
            Console.WriteLine(position);
            
            cRDT.InsertCharacter(ch, line, idx);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLK
{
    public class Block
    {
        int x;
        int y;
        public const int BlockWidth = 31;
        public const int BlockHeight = 34;
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        public Block(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}

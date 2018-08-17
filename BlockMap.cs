using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LLK
{
        public enum Direction { origin = -1, up = 0, left, down, right };
        public struct CornerPoint           //转折点
        {
            public int x;
            public int y;
            public Direction direction;
            public CornerPoint(int x, int y, Direction d)
            {
                this.x = x;
                this.y = y;
                this.direction = d;
            }
        }
        /// <summary>
        /// 对应当前连连看得布局，每个单元格为一个Blcok对象，block中0表示空，>=1表示某个图形
        /// </summary>
        class BlockMap
        {
            public const int Width = 8;
            public const int Height = 7;
            const int totalImages = 39;//图样总数
            int[,] blocks;
            public int this[int i, int j]
            {
                get
                {
                    return this.blocks[i, j];
                }
            }
            public BlockMap()
            {
                blocks = new int[Height + 2, Width + 2];
                InitializeBlocks();
            }
            /// <summary>
            /// 将地图中的图形进行初始化，保证：
            /// 1.图形是成对的
            /// 2.边界外面一圈全部是0，这主要是为了寻路方便
            /// </summary>
            private void InitializeBlocks()
            {
                Random ran = new Random(DateTime.Now.Millisecond);
                int r;
                //随机对每行块的图形编号进行初始化，每行图形首尾对称分布
                for (int i = 1; i < Height + 1; i++)
                {
                    for (int j = 1; j < Width / 2 + 1; j++)
                    {
                        r = ran.Next(totalImages) + 1;           //  0~~38 +1
                        blocks[i, j] = blocks[i, Width + 1 - j] = r;
                    }
                }
                DeOrderMap();
            }
            /// <summary>
            /// 对地图中的图块打乱顺序,依次对二维地图中的每一块,随机选择一块与之交换,达到打乱顺序的目的
            /// </summary>
            public void DeOrderMap()
            {
                Random ran = new Random(DateTime.Now.Millisecond);
                int r;
                int temp;
                for (int i = 1; i < Height + 1; i++)
                {
                    for (int j = 1; j < Width + 1; j++)
                    {
                        r = ran.Next(i * Width + j);       //0到当前位置（与之前的随机位置交换）
                        temp = blocks[i, j];
                        //避免取到边界
                        int h = r / Width;
                        int w = r % Width;
                        if (h == 0) h = 1;
                        if (w == 0) w = 1;
                        blocks[i, j] = blocks[h, w];
                        blocks[h, w] = temp;
                    }
                }
            }
            public override string ToString()
            {
                string info = "";
                for (int i = 0; i < Height + 1; i++)
                {
                    for (int j = 0; j < Width + 1; j++)
                    {
                        info += blocks[i, j] + " ";
                    }
                    info += "\n";
                }
                return info;
            }
            public void ClearBlock(Block b)      //消去该块的图片
            {
                blocks[b.Y, b.X] = 0;
            }


            /// <summary>
            /// 判断地图中指定的两个位置上的图形能否匹配消除,并将选中的转折点集合返回
            /// </summary>
            /// <param name="x1"></param>
            /// <param name="y1"></param>
            /// <param name="x2"></param>
            /// <param name="y2"></param>
            public bool LinkMatch(int x1, int y1, int x2, int y2, out List<CornerPoint> cps)
            {
                int x, y;
                cps = new List<CornerPoint>(2);
                if (CanLinkDirectly(x1, y1, x2, y2)) return true;//直接可连
                Stack<CornerPoint> stack = new Stack<CornerPoint>();//栈
                //按照上、左、下、右的顺序将从(x1,y1)出发所有可能的第一个转折点进栈
                //上
                y = y1 - 1;
                while (y >= 0 && blocks[y, x1] == 0)
                {
                    stack.Push(new CornerPoint(x1, y, Direction.up));
                    y--;
                }
                //左
                x = x1 - 1;
                while (x >= 0 && blocks[y1, x] == 0)
                {
                    stack.Push(new CornerPoint(x, y1, Direction.left));
                    x--;
                }

                //下
                y = y1 + 1;
                while (y <= Height + 1 && blocks[y, x1] == 0)
                {
                    stack.Push(new CornerPoint(x1, y, Direction.down));
                    y++;
                }
                //右
                x = x1 + 1;
                while (x <= Width + 1 && blocks[y1, x] == 0)
                {
                    stack.Push(new CornerPoint(x, y1, Direction.right));
                    x++;
                }

                while (stack.Count > 0)
                {
                    cps.Clear();      //每次清空
                    CornerPoint cp = stack.Pop();//取出顶上一个点               
                    if (CanLinkDirectly(cp.x, cp.y, x2, y2)) //通过一个转折点可连 
                    {
                        cps.Add(cp);
                        return true;
                    }
                    cps.Add(cp);//将第一个候选的转折点进结果栈
                    Stack<CornerPoint> second = new Stack<CornerPoint>();//用于作为候补第二转折点

                    if (cp.direction == Direction.up || cp.direction == Direction.down)//第一条折线是垂直方向，后继折线只能是水平方向
                    {
                        //左
                        x = cp.x - 1;
                        while (x >= 0 && blocks[cp.y, x] == 0)
                        {
                            second.Push(new CornerPoint(x, cp.y, Direction.left));
                            x--;
                        }
                        //右
                        x = cp.x + 1;
                        while (x <= Width + 1 && blocks[cp.y, x] == 0)
                        {
                            second.Push(new CornerPoint(x, cp.y, Direction.right));
                            x++;
                        }
                    }
                    else
                    {
                        //上
                        y = cp.y - 1;
                        while (y >= 0 && blocks[y, cp.x] == 0)
                        {
                            second.Push(new CornerPoint(cp.x, y, Direction.up));
                            y--;
                        }
                        //下
                        y = cp.y + 1;
                        while (y <= Height + 1 && blocks[y, cp.x] == 0)
                        {
                            second.Push(new CornerPoint(cp.x, y, Direction.down));
                            y++;
                        }
                    }
                    while (second.Count > 0)
                    {
                        cp = second.Pop();
                        if (cp.x == x2 && cp.y == y2)//通过一个折点就可直接连接
                            return true;
                        else
                        {
                            if (CanLinkDirectly(cp.x, cp.y, x2, y2))//如果从该这点可以直连到第二个图形则找到通路
                            {
                                cps.Add(cp);
                                return true;
                            }
                        }

                    }

                }
                return false;
            }
            /// <summary>
            /// 能够直连必须保证x1==x2||y1==y2且两个坐标间地图均为空（无块）
            /// </summary>
            /// <param name="x1"></param>
            /// <param name="y1"></param>
            /// <param name="x2"></param>
            /// <param name="y2"></param>
            /// <returns></returns>
            private bool CanLinkDirectly(int x1, int y1, int x2, int y2)       //是否可直线连接
            {
                int x, y;
                if (x1 == x2)      //同列
                {
                    if (y1 < y2)
                    {
                        y = y1 + 1;
                        while (y < y2)
                        {
                            if (blocks[y, x1] != 0) return false;
                            y++;
                        }
                        return true;
                    }
                    else
                    {
                        y = y1 - 1;
                        while (y > y2)
                        {
                            if (blocks[y, x1] != 0) return false;
                            y--;
                        }
                        return true;
                    }
                }
                else if (y1 == y2)   //同行
                {
                    if (x1 < x2)
                    {
                        x = x1 + 1;
                        while (x < x2)
                        {
                            if (blocks[y1, x] != 0) return false;
                            x++;
                        }
                        return true;
                    }
                    else
                    {
                        x = x1 - 1;
                        while (x > x2)
                        {
                            if (blocks[y1, x] != 0) return false;
                            x--;
                        }
                        return true;
                    }
                }
                return false;
            }
            
            /// <summary>
            /// 返回当前地图最佳显示区域
            /// </summary>
            /// <returns></returns>
            public Size GetMapSize()
            {
                return new Size((Width + 2) * Block.BlockWidth, (Height + 2) * Block.BlockHeight);
            }
            public bool IsWin()         //是否全部块（地图）为空
            {
                bool st=true;
                for(int i=0;i<Height+2;i++)
                {
                    for(int j=0;j<Width+2;j++)
                    {
                        if(blocks[i,j]!=0)
                        {
                            st = false;
                        }
                    }
                }
                return st;
            }
        }
    
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.IO.Compression;
namespace LLK
{

    public partial class Form1 : Form
    {
        BlockMap map = new BlockMap();
        Block first = null;//第一个被点击的图形坐标
        Block second = null;//第二个被点击的图形坐标

        Assembly assembly;
        ResourceManager rmManager;
        private static int score = 0;    //设置分数;
        private int xx = 101;            //设置时间
        bool isstart = true;             //设置状态（游戏暂停/运行）
        private static int san;          //求助次数

        //设置线程
        private Thread t1 = null;
        public delegate void SetControl();


        private void ChangTextBox2()             //显示游戏时间
        {
            textBox2.Text = xx.ToString();
        }
        private void ChangState(string state)   //显示游戏状态
        {
            label4.Text = state;
        }
        private void ChangSan()                //显示剩余求助次数
        {
            label6.Text = san.ToString();
        }
        private void ChangTextBox()            //显示游戏得分
        {
            textBox1.Text = score.ToString();
        }

        private void SS1()    //时间线程
        {
            for (; ; )
            {
                if (xx > 0)
                {
                    xx--;
                    Thread.Sleep(1000);
                    textBox2.Invoke(new SetControl(ChangTextBox2));
                }
                else
                {
                    ChangState("时间耗尽"); 
                    MessageBox.Show("游戏时间耗尽！");
                    t1.Abort();                //并没有真正abort
                }

            }
        }


        public Form1()           //不显示外观
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)           //被调用，显示外观
        {
            assembly = Assembly.GetExecutingAssembly();
            rmManager = new ResourceManager("LLK.images", assembly);
            Size size = map.GetMapSize();
            this.pictureBox1.Width = size.Width;
            this.pictureBox1.Height = size.Height;
            this.Width = this.pictureBox1.Width + 300;
            this.Height = this.pictureBox1.Height + 200;
            san = 3;      //设置求助次数
            if (t1 == null)
            {
                t1 = new Thread(new ThreadStart(SS1));
            }
            t1.Start();


        }
        private void PainMap(Graphics g)             //图
        {
            ResourceManager reManager;
            reManager = Images.ResourceManager;
            Image image = null;
            for (int h = 1; h < BlockMap.Height + 1; h++)
            {
                for (int w = 1; w < BlockMap.Width + 1; w++)
                {
                    if (map[h, w] > 0)
                    {
                        image = (Bitmap)rmManager.GetObject("_" + map[h, w]);
                        g.DrawImage(image, Block.BlockWidth * w, Block.BlockHeight * h);
                    }
                }
                if (first != null)
                {
                    if (map[first.Y, first.X] > 0)
                    {
                        image = (Bitmap)rmManager.GetObject("_" + map[first.Y, first.X] + "_L2");
                        g.DrawImage(image, Block.BlockWidth * first.X, Block.BlockHeight * first.Y);
                    }
                }
                if (second != null)
                {
                    if (map[second.Y, second.X] > 0)
                    {
                        image = (Bitmap)rmManager.GetObject("_" + map[second.Y, second.X] + "_L2");
                        g.DrawImage(image, Block.BlockWidth * second.X, Block.BlockHeight * second.Y);
                    }
                }
            }
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            PainMap(e.Graphics);
        }
        private void button1_Click(object sender, EventArgs e)        //打乱顺序
        {
            if (xx <= 0)      //时间耗尽，不可进行该操作
                return;

            this.map.DeOrderMap();
            this.first = second = null;
            this.pictureBox1.Invalidate();//要求图片失效

        }
        private void button2_Click(object sender, EventArgs e)         //重新开始
        {
            if (map != null)
                map = null;
            map = new BlockMap();
            this.first = second = null;
            this.pictureBox1.Invalidate();

            //修改分数，时间，求助次数，状态
            score = 0;
            xx = 100;
            san = 3;
            button3.Text = "暂  停";
            ChangSan();
            ChangTextBox();
            ChangTextBox2();
            ChangState("运行");
            isstart = true;


            //MessageBox.Show(t1.ThreadState.ToString());
            //线程停止
            if (t1.ThreadState == ThreadState.Stopped||t1.ThreadState == ThreadState.Aborted)
            {
                t1 = new Thread(new ThreadStart(SS1));
                t1.Start();
            }
            //线程挂起
            if (t1.ThreadState == ThreadState.Suspended)
                t1.Resume();


        }
        private void button3_Click(object sender, EventArgs e)            //暂停/继续
        {

            if (t1 == null || xx <= 0)  //时间耗尽 无法操作
                return;

            if (isstart == true)
            {
                button3.Text = "继  续";
                isstart = false;
                ChangState("暂停");
                t1.Suspend();
            }
            else
            {
                button3.Text = "暂  停";
                ChangState("运行");
                isstart = true;
                t1.Resume();
            }
        }
        private void button4_Click(object sender, EventArgs e)   //求助
        {
            if (xx <= 0 || san <= 0)      //时间耗尽，或剩余求助次数为0，不可进行该操作
                return;
            AiFor();
            //剩余求助次数减1
            san--;
            ChangSan();
        }
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (xx <= 0 || isstart == false)  //时间耗尽或者游戏暂停时 不能操作
                return;

            int w = e.X / Block.BlockWidth;
            int h = e.Y / Block.BlockHeight;
            if (first == null)
            {
                first = new Block(w, h); second = null;
                this.pictureBox1.Invalidate();
                return;
            }
            if (map[first.Y, first.X] == map[h, w] && (first.X != w || first.Y != h))
            {
                second = new Block(w, h);
                Pen pen = new Pen(Color.Red, 2);
                Graphics g = this.pictureBox1.CreateGraphics();
                Link(first, second, g, pen);
                this.pictureBox1.Invalidate();
            }
            else
            {
                first = new Block(w, h);
                second = null;
                this.pictureBox1.Invalidate();//图片失效
                return;
            }
            //游戏完成
            if (map.IsWin() == true)
            {
                ChangState("完成");
                MessageBox.Show("游戏已通过！\n 所用时间：" + (100 - xx) + "s\n即将进入下一局游戏！");
                button2_Click(sender, e);
            }
        }
        public void AiFor()
        {
            for (int i = 1; i < BlockMap.Height + 1; i++)
            {
                for (int j = 1; j < BlockMap.Width + 1; j++)
                {
                    if (map[i, j] != 0)
                    {
                        for (int m = i; m < BlockMap.Height + 1; m++)
                        {
                            int n = 1;
                            if (m == i) n = j + 1;
                            for (; n < BlockMap.Width + 1; n++)
                            {
                                if (map[m, n] == map[i, j] && map[m, n] != 0)
                                {
                                    AiPlay(j, i);
                                    if (AiPlay(n, m))
                                    {
                                        //Thread.Sleep(500);
                                        return;
                                    }
                                    //MessageBox.Show(i+" "+j+"=="+m+" "+n);
                                }

                            }
                        }
                    }
                }
            }
        }
        public bool AiPlay(int w, int h)
        {
            if (first == null)
            {
                first = new Block(w, h); second = null;
                this.pictureBox1.Invalidate();
                return false;
            }
            if (map[first.Y, first.X] == map[h, w] && (first.X != w || first.Y != h))
            {
                int p = score;
                second = new Block(w, h);
                Pen pen = new Pen(Color.Red, 2);
                Graphics g = this.pictureBox1.CreateGraphics();
                Link(first, second, g, pen);
                this.pictureBox1.Invalidate();
                if (score > p)
                    return true;
                else
                    return false;
            }
            else
            {
                first = new Block(w, h);
                second = null;
                this.pictureBox1.Invalidate();//图片失效
                return false;
            }

            //游戏完成
            if (map.IsWin() == true)
            {
                ChangState("完成");
                MessageBox.Show("游戏已通过！\n 所用时间：" + (100 - xx) + "s\n即将进入下一局游戏！");
            }
        }
        /// <summary>
        /// 连线消除
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="g"></param>
        /// <param name="p"></param>
        public void Link(Block first, Block second, Graphics g, Pen p)
        {
            List<CornerPoint> cps = null;
            if (map.LinkMatch(first.X, first.Y, second.X, second.Y, out cps))
            {
                Point[] ps = new Point[2 + cps.Count];
                ps[0] = new Point(first.X * Block.BlockWidth + Block.BlockWidth / 2, first.Y * Block.BlockHeight + Block.BlockHeight / 2);  //起始中心点
                int i = 1;
                while (i <= cps.Count)
                {
                    ps[i] = new Point(cps[i - 1].x * Block.BlockWidth + Block.BlockWidth / 2, cps[i - 1].y * Block.BlockHeight + Block.BlockHeight / 2);
                    i++;
                }
                ps[i] = new Point(second.X * Block.BlockWidth + Block.BlockWidth / 2, second.Y * Block.BlockHeight + Block.BlockHeight / 2);  //结束中心点
                g.DrawLines(p, ps);//画线
                Thread.Sleep(500);//画完线后线程睡0.5秒
                map.ClearBlock(first);
                map.ClearBlock(second);//消去这两个图形

                //修改得分
                score += 100;
                ChangTextBox();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //当线程处于挂起状态时（暂停），需先将其恢复，才能Abort()
            if (t1.ThreadState == ThreadState.Suspended)
                t1.Resume();

            if (t1.ThreadState != ThreadState.Aborted)
                t1.Abort();

            MessageBox.Show("关闭游戏！");
        }


    }
}

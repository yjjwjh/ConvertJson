using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ConvertJson.linkObj;
using Newtonsoft.Json;
using ConvertJson.linkRule;
using ConvertJson.product;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using ConvertJson.common;

namespace ConvertJson
{
    public partial class Form1 : Form
    {
        //1.声明自适应类实例
        AutoSizeFormClass asc = new AutoSizeFormClass();
        private List<LinkObj> linkObjList;
        private List<LinkRule> linkRuleList;
        private Product newProduct;
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 程序加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);//窗口变化自适应
            //测试结束
            //读取配置文件linkObj.json和linkRule.json文件
            // 获取应用程序的当前工作目录。
            string basePath = System.IO.Directory.GetCurrentDirectory();
            //linObj.json文件路径
            string linkObjPath = basePath + "//linkObj.json";
            string linkRulePath = basePath + "//linkRule.json";
            if (!File.Exists(linkObjPath)) //判断文件是否存在  
            {
                //不存在
                MessageBox.Show("丢失linkObj.json文件");
                return;
            }
            if (!File.Exists(linkRulePath)) //判断文件是否存在  
            {
                //不存在
                MessageBox.Show("丢失linkRule.json文件");
                return;
            }
            //读取json文件到字符串中
            string linkObjJson = GetJsonStr(linkObjPath);
            string linkRuleJson = GetJsonStr(linkRulePath);
            //进行反序列化
            linkObjList = JsonConvert.DeserializeObject<List<LinkObj>>(linkObjJson);
            linkRuleList = JsonConvert.DeserializeObject<List<LinkRule>>(linkRuleJson);
        }
        /// <summary>
        /// 计算孔位
        /// </summary>
        /// <param name="pa"></param>
        /// <param name="pb"></param>
        /// <returns></returns>
        private List<Hole> resolveLinkRule(PA pa,PB pb) 
        {
            List<Hole> holes = new List<Hole>();
            //获取交界面长度
            float bl = (float)Math.Sqrt(Math.Pow(double.Parse(pa.x) - double.Parse(pb.x), 2) + Math.Pow(double.Parse(pa.y) - double.Parse(pb.y), 2));

            //获取计算条件
            foreach (LinkRule linkRule in linkRuleList) 
            {
                List<LJ> ljs = linkRule.lj;
                for (int j=0;j<ljs.Count;j++) 
                {
                    //获取判断条件字符串
                    string tjStr = ljs[j].tj;
                    //获取字符串中的数字
                    float value= GetNumberInt(tjStr);
                    //获取pj1，pj2
                    string pj1Str = ljs[j].pj1;
                    string pj2Str = ljs[j].pj2;
                    if (tjStr.Contains("<") || tjStr.Contains("<=") || tjStr.Contains(">=") || tjStr.Contains(">"))
                    {
                        tjStr = tjStr.Replace("bl", "{0}");
                        tjStr = string.Format(tjStr, bl);
                        DataTable dt = new DataTable();
                        string isTrue = dt.Compute(tjStr, "true").ToString();
                        if (bool.Parse(isTrue))
                        {
                            //pj1和pj2没有
                            if ((pj1Str == null || pj1Str == string.Empty) && (pj2Str == null || pj2Str == string.Empty))
                            {
                                return null;
                            }
                            //pj1，以','分割
                            # region pj1
                            string[] pj1Array = pj1Str.Split(',');
                            for (int k = 1; k < pj1Array.Length; k++)
                            {
                                Hole hole=ccc(pj1Array[k], pa,pb, pj1Array[0],bl,dt);
                                holes.Add(hole);
                            }
                            #endregion
                            //pj2
                            string[] pj2Array = pj2Str.Split(',');
                            for (int k = 1; k < pj2Array.Length; k++)
                            {
                                Hole hole = ccc(pj2Array[k], pa, pb, pj2Array[0], bl, dt);
                                holes.Add(hole);
                            }
                            break;
                        }
                    }
                    else if (tjStr=="1") 
                    {
                        DataTable dt = new DataTable();
                        //pj1和pj2没有
                        if ((pj1Str == null || pj1Str == string.Empty) && (pj2Str == null || pj2Str == string.Empty))
                        {
                            return null;
                        }
                        //pj1，以','分割
                        #region pj1
                        string[] pj1Array = pj1Str.Split(',');
                        for (int k = 1; k < pj1Array.Length; k++)
                        {
                            Hole hole = ccc(pj1Array[k], pa, pb, pj1Array[0], bl, dt);
                            holes.Add(hole);
                        }
                        #endregion
                        //pj2
                        string[] pj2Array = pj2Str.Split(',');
                        for (int k = 1; k < pj2Array.Length; k++)
                        {
                            Hole hole = ccc(pj2Array[k], pa, pb, pj2Array[0], bl, dt);
                            holes.Add(hole);
                        }
                        break;
                    }
                    
                }
            
            }
            return holes;
        }

        private Hole ccc(string cfStr, PA pa,PB pb,string mc,float bl,DataTable dt) 
        {
            //拿到公式
            //string cfStr = pj1Array[k];
            //声明
            Hole hole = new Hole();
            hole.MC = mc;
            //分解计算公司
            //string[] cfArray=cfStr.Split('+', '-');
           // cfStr = cfStr.Replace("pa", "{0}");
            WZ wz = new WZ();
            if (pa.x == pa.x)//x轴坐标相等，y轴计算
            {
                wz.x = float.Parse(pa.x);
                wz.z = float.Parse(pa.z);
                if (cfStr.Contains("pa") && cfStr.Contains("bl"))
                {
                    cfStr = cfStr.Replace("pa", "{0}");
                    cfStr = cfStr.Replace("bl", "{1}");
                    if (cfStr.Contains("Math.floor"))
                    {
                        cfStr = cfStr.Replace("Math.floor", "");
                    }
                    cfStr = string.Format(cfStr, double.Parse(pa.y), bl);
                    dt = new DataTable();
                    wz.y = (float)Math.Floor(float.Parse(dt.Compute(cfStr, "").ToString()));
                }
                else if (cfStr.Contains("pb") && cfStr.Contains("bl"))
                {
                    cfStr = cfStr.Replace("pb", "{0}");
                    cfStr = cfStr.Replace("bl", "{1}");
                    if (cfStr.Contains("Math.floor"))
                    {
                        cfStr = cfStr.Replace("Math.floor", "");
                    }
                    cfStr = string.Format(cfStr, double.Parse(pb.y), bl);
                    dt = new DataTable();
                    wz.y = (float)Math.Floor(float.Parse(dt.Compute(cfStr, "").ToString()));
                }
                else if (cfStr.Contains("pa"))
                {
                    cfStr = cfStr.Replace("pa", "{0}");
                    if (cfStr.Contains("Math.floor"))
                    {
                        cfStr = cfStr.Replace("Math.floor", "");
                    }
                    cfStr = string.Format(cfStr, double.Parse(pa.y));
                    dt = new DataTable();
                    wz.y = (float)Math.Floor(float.Parse(dt.Compute(cfStr, "").ToString()));
                }
                else if (cfStr.Contains("pb"))
                {
                    cfStr = cfStr.Replace("pb", "{0}");
                    if (cfStr.Contains("Math.floor"))
                    {
                        cfStr = cfStr.Replace("Math.floor", "");
                    }
                    cfStr = string.Format(cfStr, double.Parse(pb.y));
                    dt = new DataTable();
                    wz.y = (float)Math.Floor(float.Parse(dt.Compute(cfStr, "").ToString()));
                }
            }
            else //y轴坐标相等，x轴计算
            {
                wz.y = float.Parse(pa.y);
                wz.z = float.Parse(pa.z);
                if (cfStr.Contains("pa") && cfStr.Contains("bl"))
                {
                    cfStr = cfStr.Replace("pa", "{0}");
                    cfStr = cfStr.Replace("bl", "{1}");
                    if (cfStr.Contains("Math.floor"))
                    {
                        cfStr = cfStr.Replace("Math.floor", "");
                    }
                    cfStr = string.Format(cfStr, double.Parse(pa.x), bl);
                    dt = new DataTable();
                    wz.x = (float)Math.Floor(float.Parse(dt.Compute(cfStr, "").ToString()));
                }
                else if (cfStr.Contains("pb") && cfStr.Contains("bl"))
                {
                    cfStr = cfStr.Replace("pb", "{0}");
                    cfStr = cfStr.Replace("bl", "{1}");
                    if (cfStr.Contains("Math.floor"))
                    {
                        cfStr = cfStr.Replace("Math.floor", "");
                    }
                    cfStr = string.Format(cfStr, double.Parse(pb.x), bl);
                    dt = new DataTable();
                    wz.x = (float)Math.Floor(float.Parse(dt.Compute(cfStr, "").ToString()));
                }
                else if (cfStr.Contains("pa"))
                {
                    cfStr = cfStr.Replace("pa", "{0}");
                    if (cfStr.Contains("Math.floor"))
                    {
                        cfStr = cfStr.Replace("Math.floor", "");
                    }
                    cfStr = string.Format(cfStr, double.Parse(pa.x));
                    dt = new DataTable();
                    wz.x = (float)Math.Floor(float.Parse(dt.Compute(cfStr, "").ToString()));
                }
                else if (cfStr.Contains("pb"))
                {
                    cfStr = cfStr.Replace("pb", "{0}");
                    if (cfStr.Contains("Math.floor"))
                    {
                        cfStr = cfStr.Replace("Math.floor", "");
                    }
                    cfStr = string.Format(cfStr, double.Parse(pb.x));
                    dt = new DataTable();
                    wz.x = (float)Math.Floor(float.Parse(dt.Compute(cfStr, "").ToString()));
                }
            }
            hole.wz = wz;
            return hole;
        }


        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "所有文件(*json)|*.json"; //设置要选择的文件的类型
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = fileDialog.FileName;//返回文件的完整路径  
                this.textBox1.Text = file;
            }
        }
        /// <summary>
        /// 开始转换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.textBox1.Text==null || this.textBox1.Text==string.Empty || "".Equals(this.textBox1.Text)) 
            {
                MessageBox.Show("请选择需要转换的文件!");
                return;
            }
            //打开文件获取jsonData对象
            string jsonStr = GetJsonStr(this.textBox1.Text);
            //进行反序列化
            Product product = JsonConvert.DeserializeObject<Product>(jsonStr);
            product = computeHole(product);
            string s = JsonConvert.SerializeObject(product);
            //string result1 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+@"\新json.json";//结果保存到桌面
            string newFilePath=ShowSaveFileDialog();
            if (newFilePath==null) 
            {
                MessageBox.Show("未选择文件路径！");
                return;
            }
            // 创建文件
            using (FileStream fs = File.OpenWrite(newFilePath))
            {
                StreamWriter sw = null;
                sw = new StreamWriter(fs);
                sw.Write(s);
                //关闭StreamWriter 
                sw.Flush();
                sw.Close();
            }
            //列表展示
            viewParts(product);
            this.newProduct = product;
            MessageBox.Show("转换成功!");

        }
        /// <summary>
        /// 列表展示
        /// </summary>
        /// <param name="product"></param>
        private void viewParts(Product product) 
        {
            this.listView1.Items.Clear();
            //获取部件
            List<Parts> partsList = product.parts;
            for (int i=0;i<partsList.Count;i++)
            {
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text= (i + 1).ToString();//序号
                listViewItem.SubItems.Add(partsList[i].MC);//名称
                listViewItem.SubItems.Add(partsList[i].PW+"*"+partsList[i].PH);//尺寸
                listViewItem.SubItems.Add(partsList[i].WL);//纹理
                this.listView1.Items.Add(listViewItem);
            }
        }
        /// <summary>
        /// 读取Json文件内容
        /// </summary>
        public static string GetJsonStr(string fileName)
        {
            string jsonStr = "";
            StreamReader sr = new StreamReader(fileName, Encoding.UTF8);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                jsonStr += line.ToString();
            }
            return jsonStr;
        }

        /// <summary>
        /// 获取字符串中的数字
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>数字</returns>
        public static float GetNumberInt(string str)
        {
            float result = 0;
            if (str != null && str != string.Empty)
            {
                string floatNumber = Regex.Replace(str, @"[^0-9,.]+", "");
                result=float.Parse(floatNumber);
            }
            return result;
        }
        /// <summary>
        /// 计算孔位信息
        /// </summary>
        /// <param name="product">产品的json对象</param>
        /// <returns></returns>
        public Product computeHole(Product product)
        {
            //string s="";
            //获取产品部件
            List<Parts> partsList= product.parts;
            //遍历部件
            foreach (Parts parts in partsList) 
            {
                //获取每个部件的所有交界面
                List<jjm> jjms = parts.JJM;
                //遍历每个交界面
                foreach (jjm j in jjms) 
                {
                    //获取交界面的起点和终点坐标
                    PA pa = j.pa;//起点
                    PB pb = j.pb;//终点
                    //获取交界面长度
                    float bl = (float)Math.Sqrt(Math.Pow(double.Parse(pa.x)- double.Parse(pb.x),2)+ Math.Pow(double.Parse(pa.y) - double.Parse(pb.y), 2));
                    //根据交界面与孔位辅助json进行判断
                    List<Hole> holes= resolveLinkRule(pa,pb);
                    j.BKGZ = holes;
                }
            
            }
            return product;
        }
        /// <summary>
        /// 保存文件对话框
        /// </summary>
        /// <returns>文件路径</returns>
        private string ShowSaveFileDialog()
        {
            //string localFilePath, fileNameExt, newFileName, FilePath; 
            SaveFileDialog sfd = new SaveFileDialog();

            //设置文件类型 
            sfd.Filter = "json文件(*.json)|*.json";

            //设置默认文件类型显示顺序 
            sfd.FilterIndex = 0;

            //保存对话框是否记忆上次打开的目录 
            sfd.RestoreDirectory = true;

            //设置默认的文件名
            sfd.FileName = "转换后的新json文件";
            //sfd.DefaultFileName = "汇总文件";// in wpf is  sfd.FileName = "YourFileName";

            //点了保存按钮进入 
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string localFilePath = sfd.FileName.ToString(); //获得文件路径 
                //string fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1); //获取文件名，不带路径
                return localFilePath;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 画图 -------部件示意图和交界面上的孔
        /// </summary>
        /// <param name="parts"></param>
        private void drawMap(Parts parts) 
        {
            //获取坐标系的长度（实际画布没有实际尺寸那么大，这里取宽高最大值）
            float xyMax = parts.PW;
            if (parts.PH > parts.PW) xyMax = parts.PH;

            panel1.Refresh();
            
            //首先确定原点
            Point centerPoint = new Point(20, 320);
            //自定义一个带有箭头的画笔
            Pen pen = new Pen(Color.Black, 1);
            pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            //得到当前窗体的Graphics对象
            Graphics g = panel1.CreateGraphics();
            //画X轴和Y轴坐标系
            g.DrawLine(pen,centerPoint,new Point(centerPoint.X+300,centerPoint.Y));
            g.DrawLine(pen, centerPoint, new Point(centerPoint.X, centerPoint.Y-300));
            //画坐标系的箭头出信息
            g.DrawString("0", this.Font, Brushes.Black, new PointF(centerPoint.X - 8, centerPoint.Y + 5));//x轴下面的标注
            g.DrawString((xyMax+100).ToString(), this.Font, Brushes.Black, new PointF(centerPoint.X +280, centerPoint.Y + 3));//x轴下面的标注
            g.DrawString((xyMax+100).ToString(), this.Font, Brushes.Black, new PointF(centerPoint.X -20, centerPoint.Y - 310));//x轴下面的标注
            //画部件
            //修改原点
            centerPoint = new Point(30,310);
            //画图比例为
            float sc = xyMax / 250;
            //画矩形
            //自定义一个带有箭头的画笔
            pen = new Pen(Color.Blue, 1);
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;
            g.DrawLine(pen, centerPoint, new Point((int)(centerPoint.X + parts.PW/sc), centerPoint.Y));//下边线
            g.DrawLine(pen, centerPoint, new Point(centerPoint.X, (int)(centerPoint.Y-parts.PH/sc)));//左边线
            g.DrawLine(pen, new Point(centerPoint.X, (int)(centerPoint.Y - parts.PH / sc)), new Point((int)(centerPoint.X + parts.PW / sc), (int)(centerPoint.Y - parts.PH / sc)));//上边线
            g.DrawLine(pen, new Point((int)(centerPoint.X + parts.PW / sc), centerPoint.Y), new Point((int)(centerPoint.X + parts.PW / sc), (int)(centerPoint.Y - parts.PH / sc)));//右边线
            //部件尺寸标注
            g.DrawString(parts.PW.ToString(), this.Font, Brushes.Blue, new PointF((int)(centerPoint.X + parts.PW / sc/2), (int)(centerPoint.Y - parts.PH / sc)-20));//部件宽度标注
            g.DrawString(parts.PH.ToString(), this.Font, Brushes.Blue, new PointF((int)(centerPoint.X + parts.PW / sc+20), (int)(centerPoint.Y - parts.PH / sc/2)));//部件宽度标注
            //交界面上的孔位
            //遍历交界面
            //自定义一个带有箭头的画笔
            pen = new Pen(Color.Red, 1);
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;
            List<jjm> jjmList = parts.JJM;
            foreach (jjm j in jjmList) 
            {
                //获取孔位hole
                List<Hole> holeList = j.BKGZ;
                //获取BM
                int bm = j.BM;
                if (bm!=0 && bm!=5)
                {
                    //获取孔在那个面 0左  1下  2右   3上
                    int flag = 0;
                    if (j.pa.x == "0" && j.pb.x == "0")//左
                    {
                        flag = 0;
                    }
                    else if (j.pa.x == "0" && j.pb.y == "0")//下
                    {
                        flag = 1;
                    }
                    else if (j.pa.y == "0" && j.pb.x != "0" && j.pb.y != "0")//右
                    {
                        flag = 2;
                    }
                    else
                    {
                        flag = 3;
                    }
                    //遍历孔位
                    foreach (Hole hole in holeList)
                    {

                        //获取孔是三合一还是木肖
                        string mc = hole.MC;
                        //获取点位
                        WZ wz = hole.wz;
                        if (mc == "三合一")
                        {
                            
                            //画孔
                            switch (flag)
                            {
                                case 0:
                                    g.DrawLine(pen, new Point(centerPoint.X, (int)(centerPoint.Y - wz.y / sc - 1)), new Point(centerPoint.X, (int)(centerPoint.Y - wz.y / sc + 1)));//左边线
                                    g.DrawLine(pen, new Point(centerPoint.X, (int)(centerPoint.Y - wz.y / sc - 1)), new Point(centerPoint.X + 10, (int)(centerPoint.Y - wz.y / sc - 1)));//上边线
                                    g.DrawLine(pen, new Point(centerPoint.X, (int)(centerPoint.Y - wz.y / sc + 1)), new Point(centerPoint.X + 10, (int)(centerPoint.Y - wz.y / sc + 1)));//下边线
                                                                                                                                                                                         //g.DrawEllipse(); 

                                    //g.DrawEllipse(pen, centerPoint.X + 10, centerPoint.Y - wz.y/sc, 2, 2);//画圆
                                    g.DrawString(wz.y.ToString(), this.Font, Brushes.Red, new Point(centerPoint.X-10, (int)(centerPoint.Y - wz.y / sc)));//部件宽度标注

                                    break;
                                case 1:
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc - 1), (int)(centerPoint.Y)), new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y)));//下边线
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc - 1), (int)(centerPoint.Y)), new Point((int)(centerPoint.X + wz.x / sc - 1), (int)(centerPoint.Y - 10)));//左边线
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y)), new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y - 10)));//右边线
                                    g.DrawString(wz.x.ToString(), this.Font, Brushes.Red, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y +5)));//部件宽度标注

                                    break;
                                case 2:
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc - 1)), new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc + 1)));//左边线
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc - 1)), new Point((int)(centerPoint.X + wz.x / sc - 10), (int)(centerPoint.Y - wz.y/sc - 1)));//上边线
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc + 1)), new Point((int)(centerPoint.X + wz.x / sc - 10), (int)(centerPoint.Y - wz.y/sc + 1)));//下边线
                                    //g.DrawEllipse(pen, centerPoint.X + wz.x / sc-10, centerPoint.Y - wz.y / sc,2,2);  
                                    //g.DrawEllipse(pen, centerPoint.X+wz.x / sc - 10, centerPoint.Y - wz.y/sc, 2, 2);//画圆
                                    g.DrawString(wz.y.ToString(), this.Font, Brushes.Red, new Point((int)(centerPoint.X + wz.x / sc + 10), (int)(centerPoint.Y - wz.y / sc)));//部件宽度标注

                                    break;
                                default:
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc - 1), (int)(centerPoint.Y + wz.y / sc)), new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y + wz.y / sc)));//下边线
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc - 1), (int)(centerPoint.Y + wz.y / sc)), new Point((int)(centerPoint.X + wz.x / sc - 1), (int)(centerPoint.Y + wz.y / sc + 10)));//左边线
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y + wz.y / sc)), new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y + wz.y / sc + 10)));//右边线
                                    g.DrawString(wz.x.ToString(), this.Font, Brushes.Red, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc-10)));//部件宽度标注
                                                                                                                                                                              //g.DrawEllipse(pen, centerPoint.X + wz.x/sc, centerPoint.Y - wz.y / sc - 10, 2, 2);
                                    continue;

                            }
                        }
                        else if (mc == "木肖")
                        {
                            //画孔
                            switch (flag)
                            {
                                case 0:
                                    //g.DrawLine(pen, new Point(centerPoint.X, (int)(centerPoint.Y - wz.y / sc - 1)), new Point(centerPoint.X, (int)(centerPoint.Y - wz.y / sc + 1)));//左边线
                                    g.DrawLine(pen, new Point(centerPoint.X, (int)(centerPoint.Y - wz.y / sc)), new Point(centerPoint.X + 5, (int)(centerPoint.Y - wz.y / sc )));//上边线
                                    //g.DrawLine(pen, new Point(centerPoint.X, (int)(centerPoint.Y - wz.y / sc + 1)), new Point(centerPoint.X + 6, (int)(centerPoint.Y - wz.y / sc + 1)));//下边线
                                    //g.DrawEllipse();   
                                    //g.DrawEllipse(pen, centerPoint.X + 6, centerPoint.Y - wz.y/sc, 2, 2);//画圆
                                     g.DrawString(wz.y.ToString(), this.Font, Brushes.Red, new Point(centerPoint.X-10, (int)(centerPoint.Y - wz.y / sc)));//部件宽度标注

                                    break;
                                case 1:
                                    //g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc - 1), (int)(centerPoint.Y)), new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y)));//下边线
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc ), (int)(centerPoint.Y)), new Point((int)(centerPoint.X + wz.x / sc ), (int)(centerPoint.Y - 5)));//左边线
                                                                                                                                                                                                //g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y)), new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y - 6)));//右边线
                                    g.DrawString(wz.x.ToString(), this.Font, Brushes.Red, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y + 5)));//部件宽度标注
                                                                                                                                                                 //g.DrawEllipse(pen, centerPoint.X + wz.x/sc, centerPoint.Y - 6, 2, 2);
                                    break;
                                case 2:
                                    //g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc - 1)), new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc + 1)));//左边线
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc)), new Point((int)(centerPoint.X + wz.x / sc - 5), (int)(centerPoint.Y - wz.y / sc)));//上边线
                                    //g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc + 1)), new Point((int)(centerPoint.X + wz.x / sc - 6), (int)(centerPoint.Y - wz.y / sc + 1)));//下边线
                                    //g.DrawEllipse(pen, centerPoint.X + wz.x / sc-6, centerPoint.Y - wz.y / sc,2,2);                                                                                                                                                                                     //g.DrawEllipse(pen, centerPoint.X+wz.x / sc - 6, centerPoint.Y - wz.y/sc, 2, 2);//画圆
                                    g.DrawString(wz.y.ToString(), this.Font, Brushes.Red, new Point((int)(centerPoint.X + wz.x / sc + 10), (int)(centerPoint.Y - wz.y / sc)));//部件宽度标注

                                    break;
                                default:
                                    //g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc - 1), (int)(centerPoint.Y + wz.y / sc)), new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y + wz.y / sc)));//下边线
                                    g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc ), (int)(centerPoint.Y + wz.y / sc)), new Point((int)(centerPoint.X + wz.x / sc - 1), (int)(centerPoint.Y + wz.y / sc + 5)));//左边线
                                                                                                                                                                                                                           //g.DrawLine(pen, new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y + wz.y / sc)), new Point((int)(centerPoint.X + wz.x / sc + 1), (int)(centerPoint.Y + wz.y / sc + 6)));//右边线
                                    g.DrawString(wz.x.ToString(), this.Font, Brushes.Red, new Point((int)(centerPoint.X + wz.x / sc), (int)(centerPoint.Y - wz.y / sc - 10)));//部件宽度标注
                                                                                                                                                                              //g.DrawEllipse(pen, centerPoint.X + wz.x/sc, centerPoint.Y - wz.y / sc - 6, 2, 2);
                                    continue;

                            }
                        }
                    }
                }
                
                
            }
            
            
            pen.Dispose();

        }
        /// <summary>
        /// 列表项单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_Click(object sender, EventArgs e)
        {
            int selectCount = listView1.SelectedItems.Count;
            if (selectCount == 1) 
            {
                //获取列表序号
                string indexStr= this.listView1.SelectedItems[0].SubItems[0].Text;
                if (indexStr!=null && indexStr!=string.Empty) 
                {
                    int index = int.Parse(indexStr);
                    //获取部件对象
                    Parts parts = this.newProduct.parts[index - 1];
                    drawMap(parts);
                }
                
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            //测试
            drawMap(null);
        }
        /// <summary>
        /// 窗口变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);//窗口变化自适应
        }
    }
}

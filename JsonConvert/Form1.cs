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

namespace ConvertJson
{
    public partial class Form1 : Form
    {
        private List<LinkObj> linkObjList;
        private List<LinkRule> linkRuleList;

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
            //测试
           // string cfStr = "pa<52";
           // float pa = 22.0f;
           // //float bl = 55;
           // cfStr = cfStr.Replace("pa", "{0}");
           // //cfStr = cfStr.Replace("bl", "{1}");
           //// cfStr = cfStr.Replace("Math.floor", "");
           // //cfStr = cfStr.Replace("Math.floor", "{2}");
           // cfStr = string.Format(cfStr, pa);
           // DataTable dt = new DataTable();
           // string s = dt.Compute(cfStr, "true").ToString();
           // if (bool.Parse(s)) 
           // {
           //     float ccc = float.Parse(s);
           // }
            
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
            string s=computeHole(product);
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
        public string computeHole(Product product)
        {
            string s="";
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
            s = JsonConvert.SerializeObject(product);
            return s;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ConvertJson.linkObj;
using Newtonsoft.Json;
using ConvertJson.linkRule;
using ConvertJson.product;

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
            Product productList = JsonConvert.DeserializeObject<Product>(jsonStr);
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
        
    }
}

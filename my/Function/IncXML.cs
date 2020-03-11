using System;
using System.Data;
using System.Web;
using System.Xml;
using System.Collections.Generic;
using System.Collections;

namespace Function
{
    public class IncXML
    {
        public XmlDocument xmlDoc;
        private string path;

        public IncXML(string Path, string root)
        {
            this.path = Path;
            LoadXml(path, root);
        }

        /// <summary>
        /// 加载xml文件,不存在则创建
        /// </summary>
        /// <param name="path">xml文件的物理路径</param>
        public void LoadXml(string path, string node_root)
        {
            xmlDoc = new XmlDocument();
            //判断xml文件是否存在
            if (!System.IO.File.Exists(path))
            {
                //创建xml 声明节点
                XmlNode xmlnode = xmlDoc.CreateNode(System.Xml.XmlNodeType.XmlDeclaration, "", "");
                //添加上述创建和 xml声明节点
                xmlDoc.AppendChild(xmlnode);
                //创建xml dbGuest 元素（根节点）
                XmlElement xmlelem = xmlDoc.CreateElement("", node_root, "");
                xmlDoc.AppendChild(xmlelem);
                try
                {
                    xmlDoc.Save(path);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                xmlDoc.Load(path);
            }
            else
            {
                //加载xml文件
                xmlDoc.Load(path);
            }
        }

        /// <summary>
        /// 添加xml子节点
        /// </summary>
        /// <param name="path">xml文件的物理路径</param>
        /// <param name="node_root">根节点名称</param>
        /// <param name="node_name">添加的子节点名称</param>
        /// <param name="node_text">子节点文本</param>
        public void addElement(string node_root, string node_name, string node_text, string att_name, string att_value)
        {
            XmlNodeList nodeList = xmlDoc.SelectSingleNode(node_root).ChildNodes;//获取bookstore节点的所有子节点
            //判断是否有节点,有节点就遍历所有子节点,看看有没有重复节点,没节点就添加一个新节点
            if (nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)//遍历所有子节点 
                {
                    XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型 
                    if (xe.GetAttribute(att_name) != att_value)
                    {
                        XmlNode xmldocSelect = xmlDoc.SelectSingleNode(node_root);  //选中根节点
                        XmlElement son_node = xmlDoc.CreateElement(node_name);    //添加子节点 
                        son_node.SetAttribute(att_name, att_value);    //设置属性
                        son_node.InnerText = node_text;    //添加节点文本
                        xmldocSelect.AppendChild(son_node);      //添加子节点
                        xmlDoc.Save(path);          //保存xml文件
                        break;
                    }
                }
            }
            else
            {
                XmlNode xmldocSelect = xmlDoc.SelectSingleNode(node_root);  //选中根节点
                XmlElement son_node = xmlDoc.CreateElement(node_name);    //添加子节点 
                son_node.SetAttribute(att_name, att_value);    //设置属性
                son_node.InnerText = node_text;    //添加节点文本
                xmldocSelect.AppendChild(son_node);      //添加子节点
                xmlDoc.Save(path);          //保存xml文件
            }
        }
        /// <summary>
        /// 修改节点的内容
        /// </summary>
        /// <param name="path">xml文件的物理路径</param>
        /// <param name="node_root">根节点名称</param>
        /// <param name="new_text">节点的新内容</param>
        /// <param name="att_name">节点的属性名</param>
        /// <param name="att_value">节点的属性值</param>
        public void UpdateElement(string node_root, string new_text, string att_name, string att_value)
        {
            XmlNodeList nodeList = xmlDoc.SelectSingleNode(node_root).ChildNodes;//获取bookstore节点的所有子节点 
            foreach (XmlNode xn in nodeList)//遍历所有子节点 
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型 
                if (xe.GetAttribute(att_name) == att_value)
                {
                    xe.InnerText = new_text;    //内容赋值
                    xmlDoc.Save(path);//保存 
                    break;
                }
            }

        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="path">xml文件的物理路径</param>
        /// <param name="node_root">根节点名称</param>
        /// <param name="att_name">节点的属性名</param>
        /// <param name="att_value">节点的属性值</param>
        public void deleteNode(string node_root, string att_name, string att_value)
        {
            XmlNodeList nodeList = xmlDoc.SelectSingleNode(node_root).ChildNodes;
            XmlNode root = xmlDoc.SelectSingleNode(node_root);
            foreach (XmlNode xn in nodeList)
            {
                XmlElement xe = (XmlElement)xn;
                if (xe.GetAttribute(att_name) == att_value)
                {
                    //xe.RemoveAttribute("name");//删除name属性 
                    xe.RemoveAll();//删除该节点的全部内容 
                    root.RemoveChild(xe);
                    xmlDoc.Save(path);//保存 
                    break;
                }
            }
        }


        public string nodevalue(string root_name,string node_name, string att_name)
        {
            XmlNode xmlNode = xmlDoc.SelectSingleNode(root_name);
            xmlNode = xmlNode.SelectSingleNode(node_name);
            return xmlNode.Attributes[att_name].Value;
        }

        public string nodetext(string root_name, string node_name)
        {
            XmlNode xmlNode = xmlDoc.SelectSingleNode(root_name);
            xmlNode = xmlNode.SelectSingleNode(node_name);
            return xmlNode.InnerText;
        }

        public Hashtable nodes(string root_name,string att_name)
        {
            XmlNode xmlNode = xmlDoc.SelectSingleNode(root_name);
            Hashtable ht = new Hashtable();
            foreach(XmlNode xn in xmlNode.ChildNodes)
            {
                
                ht.Add(xn.InnerText, xn.Attributes[att_name].Value);
            }
            return ht;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExtractorForWebUI.WebServices;

public class WebWriter : XmlTextWriter
{
    public WebWriter(Stream s) : base(s, Encoding.UTF8)
    {

    }

    public void ElementWithAttribute(string element, string attribute, string attributeValue)
    {
        base.WriteStartElement(element);
        base.WriteAttributeString(attribute, attributeValue);
        base.WriteString(" ");
        base.WriteEndElement();
    }

    public void ElementWithAttribute(string element, params (string attribute, string attributeValue)[] t0)
    {
        base.WriteStartElement(element);
        foreach (var (attribute, attributeValue) in t0)
            base.WriteAttributeString(attribute, attributeValue);
        base.WriteString(" ");
        base.WriteEndElement();
    }

    public void ElementWithAttribute(string element, string content, params (string attribute, string attributeValue)[] t0)
    {
        base.WriteStartElement(element);
        foreach (var (attribute, attributeValue) in t0)
            base.WriteAttributeString(attribute, attributeValue);
        base.WriteString(content);
        base.WriteEndElement();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Attribute(string name)
    {
        base.WriteAttributeString(name, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Attribute(string name, string value)
    {
        if (value != null)
            base.WriteAttributeString(name, value);
    }

    public void Input(string lable, string type, string name = null, string id = null)
    {
        this.WriteStartElement("div");
        this.WriteStartElement("label");
        this.WriteString(lable);
        {
            this.WriteStartElement("input");
            this.Attribute("name", name);
            this.Attribute("type", type);
            this.Attribute("id", id);
            this.Attribute("value", "hello");
            this.WriteEndElement();
        }
        this.WriteEndElement();
        this.WriteEndElement();
    }

    public void JS(string file)
    {
        base.WriteStartElement("script");
        base.WriteAttributeString("type", "text/javascript");
        base.WriteAttributeString("src", file);
        base.WriteString(" ");
        base.WriteEndElement();
    }
}
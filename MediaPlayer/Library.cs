using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Reflection;

namespace MediaPlayer
{
    public class Media
    {
        public String Title { get; set; }
        public String Duration { get; set; }
        public String Type { get; set; }
        public String Path { get; set; }
        public MediaElement media { get; set; }
    }

    class Library
    {
        XElement core;
        Media media;
        public List<Media> list_media;
        public bool init = false;

        public Library()
        {
            list_media = new List<Media>();
            //test();
            get_File();
            //Load file XML
            //Recupere la liste
            //On remplis l'interface avec la liste
        }

        public void test()
        {
            init_Elements();
            create_Media("test1", "1:30", "video", "c:/sdds/ds/");
            create_Media("test2", "2:30", "music", "c:/sdds/ds/");
            create_Media("test3", "3:30", "image", "c:/sdds/ds/");
            create_Media("test4", "2:45", "video", "c:/sdds/ds/");
            create_Media("test5", "1:30", "image", "c:/sdds/ds/");
            save_To_File();
        }

        //SI ADD MEDIA tu creer un MEDIA et tu fais un ADD MEDIA

        public Media create_Media(String Title, String Duration, String Type, String Path)
        {
            media = new Media();
            media.Title = Title;
            media.Duration = Duration;
            media.Type = Type;
            media.Path = Path;
           // add_Media();
            return media;
        }

        public void init_Elements()
        {
            init = true;
            core = new XElement("Library");
        }

        public Media add_Media(Media m)
        {
            XElement newEl = new XElement("Media", new XElement("Title", m.Title), new XElement("Duration", m.Duration), new XElement("Type", m.Type), new XElement("Path", m.Path));
            core.Add(newEl);
            list_media.Add(m);
            return m;
        }

        public void get_File()
        {
            if (System.IO.File.Exists("Library.xml"))
            {
                XDocument xmlFile = XDocument.Load(@"Library.xml");
                if (xmlFile != null)
                {
                    var buffer = from lv1 in xmlFile.Descendants("Media")
                                 select new Media()
                                 {
                                     Title = lv1.Element("Title").Value,
                                     Duration = lv1.Element("Duration").Value,
                                     Type = lv1.Element("Type").Value,
                                     Path = lv1.Element("Path").Value
                                 };
                    if (buffer != null)
                    {
                        list_media = buffer.ToList();
                        init_Elements();
                        foreach (Media m in list_media)
                        {
                            XElement newEl = new XElement("Media", new XElement("Title", m.Title), new XElement("Duration", m.Duration), new XElement("Type", m.Type), new XElement("Path", m.Path));
                            core.Add(newEl);
                        }
                    }
                }
            }
        }

        public void save_To_File()
        {
            if (core != null)
                core.Save("Library.xml");
        }
    }
}

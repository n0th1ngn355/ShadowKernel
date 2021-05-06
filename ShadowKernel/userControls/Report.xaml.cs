using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace ShadowKernel.userControls
{
    public partial class Report : UserControl
    {
        public Report()
        {
            InitializeComponent();
        }
        public struct MyData
        {
            public string Color { set; get; }
            public string Category { set; get; }
            public string Protection { set; get; }
            public int Questions { set; get; }
            public int Answered{ set; get; }
            public int Yes{ set; get; }
            public int No{ set; get; }
            public int NA{ set; get; }
        }

    }
}

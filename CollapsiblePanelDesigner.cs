using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using System.Diagnostics;

/// found at http://www.codeproject.com/Articles/53318/C-Custom-Control-Featuring-a-Collapsible-Panel

namespace OVT.CustomControls
{
    public class CollapsiblePanelDesigner : ParentControlDesigner
    {

        public override System.ComponentModel.Design.DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection collection = new DesignerActionListCollection();
                if (this.Control != null && this.Control is CollapsiblePanel)
                {
                    CollapsiblePanel panel = (CollapsiblePanel)this.Control;
                    if (!String.IsNullOrEmpty(panel.Name))
                    {
                        if (String.IsNullOrEmpty(panel.HeaderText))
                            panel.HeaderText = panel.Name;
                    }
                }

                collection.Add(new CollapsiblePanelActionList(this.Control));
                
                return collection;
            }
        }

       


        
    }
}

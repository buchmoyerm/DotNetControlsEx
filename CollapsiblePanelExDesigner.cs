using System;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

/// found at http://www.codeproject.com/Articles/53318/C-Custom-Control-Featuring-a-Collapsible-Panel

namespace DotNetControlsEx
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

    public class CollapsiblePanelExDesigner : ParentControlDesigner
    {

        public override System.ComponentModel.Design.DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection collection = new DesignerActionListCollection();
                if (this.Control != null && this.Control is CollapsiblePanelEx)
                {
                    CollapsiblePanelEx panel = (CollapsiblePanelEx)this.Control;
                    if (!String.IsNullOrEmpty(panel.Name))
                    {
                        if (String.IsNullOrEmpty(panel.LeftHeaderText))
                            panel.LeftHeaderText = panel.Name;
                    }
                }

                collection.Add(new CollapsiblePanelExActionList(this.Control));

                return collection;
            }
        }





    }
}

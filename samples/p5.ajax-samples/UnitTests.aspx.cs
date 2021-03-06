/*
 * Phosphorus Five, copyright 2014 - 2017, Thomas Hansen, thomas@gaiasoul.com
 * 
 * This file is part of Phosphorus Five.
 *
 * Phosphorus Five is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 3, as published by
 * the Free Software Foundation.
 *
 *
 * Phosphorus Five is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Phosphorus Five.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * If you cannot for some reasons use the GPL license, Phosphorus
 * Five is also commercially available under Quid Pro Quo terms. Check 
 * out our website at http://gaiasoul.com for more details.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using p5.ajax.core;

namespace p5.samples
{
    using p5 = ajax.widgets;

    public class UnitTests : AjaxPage
    {
        [WebMethod]
        protected void sandbox_invoke_empty_onclick (p5.Literal literal, EventArgs e)
        {
            if (literal.ID != "sandbox_invoke_empty")
                throw new ApplicationException ("wrong id of element on server");
        }

        [WebMethod]
        protected void sandbox_invoke_exception_onclick (p5.Literal literal, EventArgs e) { throw new ApplicationException ("this is an intentional error"); }

        //[WebMethod] intentionally commented out
        protected void sandbox_invoke_no_webmethod_onclick (p5.Literal literal, EventArgs e) { }

        [WebMethod]
        protected void sandbox_invoke_normal_onclick (p5.Literal literal, EventArgs e) { }

        [WebMethod]
        protected void sandbox_invoke_multiple_onclick (p5.Literal literal, EventArgs e)
        {
            literal.innerValue += "x";
            Thread.Sleep (100);
        }

        [WebMethod]
        protected void sandbox_invoke_javascript_onclick (p5.Literal literal, EventArgs e)
        {
            literal.innerValue = Page.Request.Params ["mumbo"] + " jumbo";
        }

        [WebMethod]
        protected void sandbox_invoke_change_content_onclick (p5.Literal literal, EventArgs e)
        {
            literal.innerValue = "new value";
        }

        [WebMethod]
        protected void sandbox_invoke_change_two_properties_onclick (p5.Literal literal, EventArgs e)
        {
            literal ["class"] = "new value 1";
            literal.innerValue = "new value 2";
        }

        [WebMethod]
        protected void sandbox_invoke_add_remove_1_onclick (p5.Literal literal, EventArgs e)
        {
            literal ["class"] = "new value 1";
        }

        [WebMethod]
        protected void sandbox_invoke_add_remove_2_onclick (p5.Literal literal, EventArgs e)
        {
            literal.DeleteAttribute ("class");
        }

        [WebMethod]
        protected void sandbox_invoke_add_remove_same_onclick (p5.Literal literal, EventArgs e)
        {
            literal ["class"] = "mumbo-jumbo";
            literal.DeleteAttribute ("class");
        }

        [WebMethod]
        protected void sandbox_invoke_change_twice_onclick (p5.Literal literal, EventArgs e)
        {
            literal ["class"] = "mumbo";
            literal ["class"] = "jumbo";
        }

        [WebMethod]
        protected void sandbox_invoke_change_markup_attribute_onclick (p5.Literal literal, EventArgs e)
        {
            literal ["class"] = "bar";
        }

        [WebMethod]
        protected void sandbox_invoke_remove_markup_attribute_onclick (p5.Literal literal, EventArgs e)
        {
            literal.DeleteAttribute ("class");
        }

        [WebMethod]
        protected void sandbox_invoke_remove_add_markup_attribute_1_onclick (p5.Literal literal, EventArgs e)
        {
            literal.DeleteAttribute ("class");
        }

        [WebMethod]
        protected void sandbox_invoke_remove_add_markup_attribute_2_onclick (p5.Literal literal, EventArgs e)
        {
            literal ["class"] = "bar";
        }

        [WebMethod]
        protected void sandbox_invoke_concatenate_long_attribute_onclick (p5.Literal literal, EventArgs e) { literal ["class"] += "qwerty"; }

        [WebMethod]
        protected void sandbox_invoke_create_concatenate_long_attribute_1_onclick (p5.Literal literal, EventArgs e) { literal ["class"] = "x1234567890"; }

        [WebMethod]
        protected void sandbox_invoke_create_concatenate_long_attribute_2_onclick (p5.Literal literal, EventArgs e) { literal ["class"] += "abcdefghijklmnopqrstuvwxyz"; }

        [WebMethod]
        protected void sandbox_invoke_change_container_child_child_onclick (p5.Literal literal, EventArgs e) { literal ["class"] += "bar"; }

        [WebMethod]
        protected void sandbox_invoke_make_container_visible_onclick (p5.Container container, EventArgs e) { container.Visible = true; }

        [WebMethod]
        protected void sandbox_invoke_make_container_visible_invisible_child_onclick (p5.Container container, EventArgs e) { container.Visible = true; }

        [WebMethod]
        protected void sandbox_invoke_make_container_visible_child_invisible_2_onclick (p5.Container container, EventArgs e) { container.Visible = true; }

        [WebMethod]
        protected void sandbox_invoke_make_container_visible_child_visible_2_onclick (p5.Container container, EventArgs e) { container.Visible = true; }

        [WebMethod]
        protected void sandbox_invoke_add_child_onclick (p5.Container container, EventArgs e)
        {
            var existing = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            if (existing.Count != 1)
                throw new ApplicationException ("widget disappeared somehow");

            if (existing [0].innerValue != "foo")
                throw new ApplicationException ("widget had wrong innerValue");

            var literal = container.CreatePersistentControl<p5.Literal> ();
            literal.Element = "strong";
            literal.innerValue = "howdy world";

            existing = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            if (existing.Count != 2)
                throw new ApplicationException ("widget disappeared somehow after insertion");

            if (existing [1].innerValue != "howdy world")
                throw new ApplicationException ("widget had wrong innerValue");
        }

        [WebMethod]
        protected void sandbox_invoke_insert_child_onclick (p5.Container container, EventArgs e)
        {
            var literal = container.CreatePersistentControl<p5.Literal> (null, 0);
            literal.Element = "strong";
            literal.innerValue = "howdy world";

            var existing = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            if (existing.Count != 2)
                throw new ApplicationException ("widget disappeared somehow after insertion");

            if (existing [0].innerValue != "howdy world")
                throw new ApplicationException ("widget had wrong innerValue");
        }

        [WebMethod]
        protected void sandbox_invoke_add_child_check_exist_1_onclick (p5.Container container, EventArgs e)
        {
            var literal = container.CreatePersistentControl<p5.Literal> ();
            literal.Element = "strong";
            literal.innerValue = "howdy world";
        }

        [WebMethod]
        protected void sandbox_invoke_add_child_check_exist_2_onclick (p5.Container container, EventArgs e)
        {
            var existing = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            if (existing.Count != 2)
                throw new ApplicationException ("widget disappeared somehow");

            if (existing [0].innerValue != "foo")
                throw new ApplicationException ("widget had wrong innerValue");

            if (existing [1].innerValue != "howdy world")
                throw new ApplicationException ("widget had wrong innerValue");

            var literal = container.CreatePersistentControl<p5.Literal> ();
            literal.Element = "strong";
            literal.innerValue = "howdy world 2";

            existing = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            if (existing.Count != 3)
                throw new ApplicationException ("widget disappeared somehow after insertion");

            if (existing [2].innerValue != "howdy world 2")
                throw new ApplicationException ("widget had wrong innerValue");
        }

        [WebMethod]
        protected void sandbox_invoke_insert_child_check_exist_1_onclick (p5.Container container, EventArgs e)
        {
            var literal = container.CreatePersistentControl<p5.Literal> (null, 0);
            literal.Element = "strong";
            literal.innerValue = "howdy world";
        }

        [WebMethod]
        protected void sandbox_invoke_insert_child_check_exist_2_onclick (p5.Container container, EventArgs e)
        {
            var existing = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            if (existing.Count != 2)
                throw new ApplicationException ("widget disappeared somehow");

            if (existing [0].innerValue != "howdy world")
                throw new ApplicationException ("widget had wrong innerValue");

            if (existing [1].innerValue != "foo")
                throw new ApplicationException ("widget had wrong innerValue");

            var literal = container.CreatePersistentControl<p5.Literal> (null, 1);
            literal.Element = "strong";
            literal.innerValue = "howdy world 2";

            existing = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            if (existing.Count != 3)
                throw new ApplicationException ("widget disappeared somehow after insertion");

            if (existing [1].innerValue != "howdy world 2")
                throw new ApplicationException ("widget had wrong innerValue");
        }

        [WebMethod]
        protected void sandbox_invoke_remove_child_onclick (p5.Container container, EventArgs e)
        {
            var literals = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            container.RemoveControlPersistent (literals [0]);
        }

        [WebMethod]
        protected void sandbox_invoke_remove_multiple_onclick (p5.Container container, EventArgs e)
        {
            var literals = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            container.RemoveControlPersistent (literals [0]);
            container.RemoveControlPersistent (literals [1]);
        }

        [WebMethod]
        protected void sandbox_invoke_append_remove_onclick (p5.Container container, EventArgs e)
        {
            var literals = new List<p5.Literal> (container.ControlsOfType<p5.Literal> ());
            container.RemoveControlPersistent (literals [0]);
            var literal = container.CreatePersistentControl<p5.Literal> (null, 0);
            literal.Element = "strong";
            literal.innerValue = "howdy world";
        }

        [WebMethod]
        protected void sandbox_invoke_remove_many_onclick (p5.Container container, EventArgs e)
        {
            // removing three controls
            container.RemoveControlPersistentAt (1); // sandbox_invoke_remove_many_2
            ((p5.Container)container.Controls [1]).RemoveControlPersistentAt (2); // sandbox_invoke_remove_many_6
            ((p5.Container)((p5.Container)container.Controls [1]).Controls [1]).RemoveControlPersistentAt (1); // sandbox_invoke_remove_many_9

            // creating two new controls

            // parent is sandbox_invoke_remove_many
            var lit1 = container.CreatePersistentControl<p5.Literal> (null, 0);
            lit1.Element = "strong";
            lit1.innerValue = "howdy";

            // parent is sandbox_invoke_remove_many_5
            var lit2 = ((p5.Container)((p5.Container)container.Controls [2]).Controls [1]).CreatePersistentControl<p5.Literal> ();
            lit2.Element = "em";
            lit2.innerValue = "world";
        }

        [WebMethod]
        protected void sandbox_invoke_add_similar_onclick (p5.Container container, EventArgs e)
        {
            // removing one controls
            container.RemoveControlPersistentAt (0); // sandbox_invoke_remove_many_2

            // adding another control with same ID
            var lit1 = container.CreatePersistentControl<p5.Literal> ("sandbox_invoke_add_similar_child", 0);
            lit1.Element = "strong";
            lit1.innerValue = "howdy";
        }

        [WebMethod]
        protected void sandbox_invoke_remove_many_verify_onclick (p5.Container container, EventArgs e)
        {
            if (container.Controls.Count != 3)
                throw new ApplicationException ("control count not correct on postback");
            if (container.Controls [2].Controls.Count != 2)
                throw new ApplicationException ("control count not correct on postback");
            if (container.Controls [2].Controls [1].Controls.Count != 3)
                throw new ApplicationException ("control count not correct on postback");
            if (((p5.Literal)container.Controls [0]).innerValue != "howdy")
                throw new ApplicationException ("control value not correct on postback");
            if (((p5.Literal)container.Controls [0]).Element != "strong")
                throw new ApplicationException ("control element not correct on postback");
            if (((p5.Literal)container.Controls [2].Controls [1].Controls [2]).innerValue != "world")
                throw new ApplicationException ("control value not correct on postback");
            if (((p5.Literal)container.Controls [2].Controls [1].Controls [2]).Element != "em")
                throw new ApplicationException ("control element not correct on postback");
        }
    }
}
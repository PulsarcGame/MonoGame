﻿using System;
using Xwt;
using Xwt.Drawing;
using System.IO;
using System.Collections.Generic;

namespace MonoGame.Tools.Pipeline
{
    class ProjectView : TreeView
    {
        private Image GetImage(bool folder)
        {
            return (folder) ? Image.FromResource("MonoGame.Tools.Pipeline.Icons.folder_closed.png") : Image.FromResource("MonoGame.Tools.Pipeline.Icons.blueprint.png");
        }

        public Image ICON_BASE = Image.FromResource("MonoGame.Tools.Pipeline.Icons.settings.png");

        public const int ID_BASE = 0, ID_FOLDER = 1, ID_FILE = 2;

        TreePosition _root;
        string _rootname;
        TreeStore _treeStore;
        MainWindow _window;

        DataField<int> _idCol = new DataField<int>();
        DataField<Image> _imgCol = new DataField<Image>();
        DataField<string> _textCol = new DataField<string>();

        public ProjectView()
        {
            _rootname = "";

            _treeStore = new TreeStore(_idCol, _imgCol, _textCol);
            this.DataSource = _treeStore;

            this.Columns.Add("", _imgCol, _textCol);
            this.HeadersVisible = false;
            this.SelectionMode = SelectionMode.Multiple;
            this.ButtonPressed += ProjectView_ButtonPressed;
            this.ButtonReleased += ProjectView_ButtonReleased;
        }

        void ProjectView_ButtonReleased (object sender, ButtonEventArgs e)
        {
            if (e.Button == PointerButton.Right)
                _window.ShowMenu((int)e.X, (int)e.Y);
        }

        void ProjectView_ButtonPressed (object sender, ButtonEventArgs e)
        {
            // TODO: Add some right click logic
            //if (e.Button == PointerButton.Right) e.Handled = true;
        }

        public void Attach(MainWindow window)
        {
            this._window = window;
        }

        public void SetRoot(string rootname)
        {
            this._rootname = rootname;
            _treeStore.GetNavigatorAt(GetRoot()).SetValue(_textCol, _rootname);
        }

        public TreePosition GetRoot()
        {
            if (_root == null)
                _root = _treeStore.AddNode().SetValue(_idCol, ID_BASE).SetValue(_imgCol, ICON_BASE).CurrentPosition;

            return _root;
        }

        public void Close()
        {
            this._rootname = "";
            _treeStore.Clear();
            _root = null;
        }

        public TreePosition GetItemFromPath(TreePosition start, string path)
        {
            string[] split = path.Split (Path.DirectorySeparatorChar);
            TreePosition pos = GetItem(start, split[0]);

            if (pos == null)
                return null;

            if (split.Length > 1)
            {
                string newpath = split[1];
                for (int i = 2; i < split.Length; i++)
                    newpath += Path.DirectorySeparatorChar + split[i];

                return GetItemFromPath(pos, newpath);
            }

            return pos;
        }

        public TreePosition GetItem(TreePosition pos, string name)
        {
            var nav = _treeStore.GetNavigatorAt(pos);

            if (nav.MoveToChild())
            {
                do
                {
                    string navname = nav.GetValue(_textCol);

                    if (navname == name)
                        return nav.CurrentPosition;
                }
                while(nav.MoveNext());
            }

            return null;
        }

        public void ExpandPath(TreePosition start, string path)
        {
            this.ExpandRow(start, false);

            string[] split = path.Split (Path.DirectorySeparatorChar);
            TreePosition pos = GetItem(start, split[0]);

            if (pos == null)
                return;

            if (split.Length > 1) {

                string newpath = split [1];
                for(int i = 2;i < split.Length;i++)
                    newpath += Path.DirectorySeparatorChar + split[i];

                ExpandPath(pos, newpath);
            }
            else
                this.ExpandRow(pos, false);
        }

        public void AddItem(TreePosition start, string path, bool exists, bool folder, string fullpath)
        {
            int id = (folder || path.Contains(Path.DirectorySeparatorChar.ToString())) ? ID_FOLDER : ID_FILE;
            Image icon = GetImage(folder || path.Contains(Path.DirectorySeparatorChar.ToString()));

            string[] split = path.Split (Path.DirectorySeparatorChar);

            TreePosition pos = GetItem(start, split[0]) ?? AddAndSort(start, id, icon, split[0]);

            if (split.Length > 1) {

                string newpath = split [1];
                for(int i = 2;i < split.Length;i++)
                    newpath += Path.DirectorySeparatorChar + split[i];

                AddItem (pos, newpath, exists, folder, fullpath);
            }
        }

        private TreePosition AddAndSort(TreePosition pos, int id, Image img, string text)
        {
            var nav = _treeStore.GetNavigatorAt(pos);

            if (nav.MoveToChild())
            {
                int count = 0;

                do
                {
                    if (nav.GetValue(_idCol) == id)
                    {
                        count++;

                        if(nav.GetValue(_textCol).CompareTo(text) >= 0)
                            return nav.InsertBefore().SetValue(_idCol, id).SetValue(_imgCol, img).SetValue(_textCol, text).CurrentPosition;
                    }
                    else if(nav.GetValue(_idCol) == ID_FILE)
                        return nav.InsertBefore().SetValue(_idCol, id).SetValue(_imgCol, img).SetValue(_textCol, text).CurrentPosition;
                }
                while(nav.MoveNext());

                if (count == 0 && id == ID_FOLDER)
                {
                    nav = _treeStore.GetNavigatorAt(pos);
                    nav.MoveToChild();
                    return nav.InsertBefore().SetValue(_idCol, id).SetValue(_imgCol, img).SetValue(_textCol, text).CurrentPosition;
                }
            }

            return _treeStore.AddNode(pos).SetValue(_idCol, id).SetValue(_imgCol, img).SetValue(_textCol, text).CurrentPosition;
        }

        public void RemoveItem(TreePosition start, string path)
        {
            string[] split = path.Split (Path.DirectorySeparatorChar);

            TreePosition pos = GetItem(start, split[0]);

            if (pos == null)
                return;

            if (split.Length > 1)
            {
                string newpath = split[1];
                for (int i = 2; i < split.Length; i++)
                    newpath += Path.DirectorySeparatorChar + split[i];

                RemoveItem(pos, path);
            }
            else
                _treeStore.GetNavigatorAt(pos).Remove();
        }

        public List<ContentItem> GetItems(TreePosition pos)
        {
            var items = new List<ContentItem>();
            var nav = _treeStore.GetNavigatorAt(pos);

            if (nav.GetValue(_idCol) == ID_FILE)
            {
                var item = _window._controller.GetItem(GetPath(nav.CurrentPosition)) as ContentItem;

                if (item != null)
                    items.Add(item);
            }

            if (nav.MoveToChild())
            {
                do
                {
                    items.AddRange(GetItems(nav.CurrentPosition));
                }
                while(nav.MoveNext());
            }

            return items;
        }

        public void GetInfo(TreePosition pos, out FileType type, out string path, out string loc)
        {
            path = GetPath(pos);

            var id = _treeStore.GetNavigatorAt(pos).GetValue(_idCol);

            if (id == ID_BASE)
            {
                loc = "";
                type = FileType.Base;
            }
            else if (id == ID_FILE)
            {
                loc = Path.GetDirectoryName(path);
                type = FileType.File;
            }
            else
            {
                loc = path;
                type = FileType.Folder;
            }
        }

        public string GetPath(TreePosition pos)
        {
            TreeNavigator nav = _treeStore.GetNavigatorAt(pos);
            string path = "";

            do
            {
                if(nav.GetValue(_idCol) != ID_BASE)
                    path = nav.GetValue(_textCol) + Path.DirectorySeparatorChar + path;
            }
            while(nav.MoveToParent());

            if(path != "")
                path = path.Remove(path.Length - 1);
            
            if (_treeStore.GetNavigatorAt(pos).GetValue(_idCol) == ID_BASE)
                path = _treeStore.GetNavigatorAt(_root).GetValue(_textCol);

            return path;
        }
    }
}


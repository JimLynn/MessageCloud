using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MessageCloud.Commanding
{
    /// <summary>
    /// Maintains a mapping between ICommand and ButtonBase
    /// objects, for the ButtonBaseExtensions class.
    /// </summary>
    internal class CommandToButtonsMap
    {
        #region Fields

        /// <summary>
        /// Maps ICommand objects to lists of ButtonBase objects.  Stores the object references
        /// as WeakReferences, so that the commands and buttons can be garbage collected as necessary.
        /// </summary>
        readonly Dictionary<WeakReference, List<WeakReference>> _map = new Dictionary<WeakReference, List<WeakReference>>();

        #endregion // Fields

        #region Internal Methods

        internal void AddButtonToMap(ButtonBase btn, ICommand cmd)
        {
            if (!ContainsCommand(cmd))
                _map.Add(new WeakReference(cmd), new List<WeakReference>());

            List<WeakReference> weakRefs = GetButtonsFromCommand(cmd);
            weakRefs.Add(new WeakReference(btn));
        }

        internal bool ContainsCommand(ICommand cmd)
        {
            return GetButtonsFromCommand(cmd) != null;
        }

        internal void ForEachButton(ICommand cmd, Action<ButtonBase> callback)
        {
            List<WeakReference> buttonRefs = GetButtonsFromCommand(cmd);
            for (int i = buttonRefs.Count - 1; i > -1; --i)
            {
                WeakReference weakRef = buttonRefs[i];
                ButtonBase btn = weakRef.Target as ButtonBase;
                if (btn != null)
                    callback(btn);
            }
        }

        internal void RemoveButtonFromMap(ButtonBase btn, ICommand cmd)
        {
            List<WeakReference> buttonRefs = this.GetButtonsFromCommand(cmd);
            if (buttonRefs == null)
                return;

            for (int i = buttonRefs.Count - 1; i > -1; --i)
            {
                WeakReference weakRef = buttonRefs[i];
                if (weakRef.Target == btn)
                {
                    buttonRefs.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion // Internal Methods

        #region Private Helpers

        List<WeakReference> GetButtonsFromCommand(ICommand cmd)
        {
            this.Prune();
            return _map.FirstOrDefault(entry => entry.Key.Target == cmd).Value;
        }

        void Prune()
        {
            List<WeakReference> cmds = _map.Keys.ToList();
            for (int cmdIndex = cmds.Count - 1; cmdIndex > -1; --cmdIndex)
            {
                WeakReference cmdRef = cmds[cmdIndex];
                if (!cmdRef.IsAlive)
                {
                    _map.Remove(cmdRef);
                }
                else
                {
                    List<WeakReference> btns = _map[cmdRef];
                    for (int btnIndex = btns.Count - 1; btnIndex > -1; --btnIndex)
                    {
                        WeakReference btnRef = btns[btnIndex];
                        if (!btnRef.IsAlive)
                            btns.RemoveAt(btnIndex);
                    }
                }
            }
        }

        #endregion // Private Helpers
    }
}
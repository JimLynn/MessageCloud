using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MessageCloud.Commanding
{
    /// <summary>
    /// Provides attached properties that extend the behavior
    /// of controls that derive from ButtonBase.
    /// </summary>
    public static class ButtonBaseExtensions
    {
        #region Command

        static readonly CommandToButtonsMap s_commandToButtonsMap = new CommandToButtonsMap();

        public static ICommand GetCommand(ButtonBase btn)
        {
            return (ICommand)btn.GetValue(CommandProperty);
        }

        public static void SetCommand(ButtonBase btn, ICommand value)
        {
            btn.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(ButtonBaseExtensions),
            new PropertyMetadata(null, OnCommandChanged));

        static void OnCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ButtonBase btn = obj as ButtonBase;
            if (btn == null)
                throw new ArgumentException("You must set the Command attached property on an element that derives from ButtonBase.");

            ICommand oldCommand = e.OldValue as ICommand;
            if (oldCommand != null)
            {
                s_commandToButtonsMap.RemoveButtonFromMap(btn, oldCommand);
                oldCommand.CanExecuteChanged -= OnCommandCanExecuteChanged;
                btn.Click -= OnButtonBaseClick;
            }

            ICommand newCommand = e.NewValue as ICommand;
            if (newCommand != null)
            {
                s_commandToButtonsMap.AddButtonToMap(btn, newCommand);
                newCommand.CanExecuteChanged += OnCommandCanExecuteChanged;
                btn.Click += OnButtonBaseClick;
            }
        }

        static void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            ICommand cmd = sender as ICommand;
            if (cmd != null && s_commandToButtonsMap.ContainsCommand(cmd))
                s_commandToButtonsMap.ForEachButton(cmd, btn =>
                {
                    object parameter = ButtonBaseExtensions.GetCommandParameter(btn);
                    btn.IsEnabled = cmd.CanExecute(parameter);
                });
        }

        static void OnButtonBaseClick(object sender, RoutedEventArgs e)
        {
            ButtonBase btn = sender as ButtonBase;
            ICommand cmd = ButtonBaseExtensions.GetCommand(btn);
            object parameter = ButtonBaseExtensions.GetCommandParameter(btn);
            if (cmd != null && cmd.CanExecute(parameter))
                cmd.Execute(parameter);
        }

        #endregion // Command

        #region CommandParameter

        public static object GetCommandParameter(ButtonBase obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(ButtonBase obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(ButtonBaseExtensions),
            new PropertyMetadata(null, OnCommandParameterChanged));

        static void OnCommandParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ButtonBase btn = obj as ButtonBase;
            if (btn == null)
                throw new ArgumentException("You must set the CommandParameter attached property on an element that derives from ButtonBase.");

            ICommand cmd = ButtonBaseExtensions.GetCommand(btn);
            if (cmd == null)
                return;

            object parameter = ButtonBaseExtensions.GetCommandParameter(btn);
            btn.IsEnabled = cmd.CanExecute(parameter);
        }

        #endregion // CommandParameter
    }
}
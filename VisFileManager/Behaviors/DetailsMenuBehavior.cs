using Microsoft.Xaml.Behaviors;
using System.Windows;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.Behaviors
{
    public class DetailsMenuBehavior : Behavior<FrameworkElement>
    {
        public IDetailsViewModel DetailsViewModel
        {
            get { return (IDetailsViewModel)GetValue(DetailsViewModelProperty); }
            set { SetValue(DetailsViewModelProperty, value); }
        }

        public static readonly DependencyProperty DetailsViewModelProperty = DependencyProperty.Register(
            "DetailsViewModel", typeof(IDetailsViewModel), typeof(DetailsMenuBehavior));

        protected override void OnAttached()
        {
            var messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            messenger.Subscribe<DetailsItemHoveredMessage>(MessageIds.DetailsItemHovered, (message) =>
            {
                var item = message.DetailsItemViewModel;

                // close higher menus
                if (DetailsViewModel.DetailsSubMenus.Count > item.Level)
                {
                    for (int i = item.Level; i < DetailsViewModel.DetailsSubMenus.Count; i++)
                    {
                        DetailsViewModel.DetailsSubMenus[i].CloseMenu();
                    }
                }

                if (item is IComplexDetailsItemViewModel)
                {
                    if (DetailsViewModel.DetailsSubMenus.Count <= item.Level)
                    {
                        for (int i = DetailsViewModel.DetailsSubMenus.Count; i < item.Level + 1; i++)
                        {
                            DetailsViewModel.DetailsSubMenus.Add(new Controls.DetailsItemSubMenuList());
                        }
                    }

                    DetailsViewModel.DetailsSubMenus[item.Level].SetItems((item as IComplexDetailsItemViewModel).ChildItems, item.Name);
                    DetailsViewModel.DetailsSubMenus[item.Level].OpenMenu(message.AbsolutePosition);
                }
                else if (item is ISimpleDetailsItemViewModel)
                {

                }
            });
        }
    }
}

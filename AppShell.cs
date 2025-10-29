using Microsoft.Maui.Controls;
using Planify.Pages;

namespace Planify
{
    public class AppShell : Shell
    {
        public AppShell()
        {
            Title = "Planify";

            Items.Add(new TabBar
            {
                Items =
                {
                    new ShellContent{ Title="Board",    Route="BoardPage",    ContentTemplate=new DataTemplate(()=> new BoardPage()) },
                    new ShellContent{ Title="Floors",   Route="FloorPage",    ContentTemplate=new DataTemplate(()=> new FloorPage()) },
                    new ShellContent{ Title="Settings", Route="SettingsPage", ContentTemplate=new DataTemplate(()=> new SettingsPage()) }
                }
            });
        }
    }
}

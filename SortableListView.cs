using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chronicle
{
    public class SortableListView : ListView
    {
        private GridViewColumnHeader lastHeaderClicked = null;
        private ListSortDirection lastDirection = ListSortDirection.Descending;

        public void GridViewColumnHeaderClicked(GridViewColumnHeader clickedHeader)
        {
            ListSortDirection direction;

            if (clickedHeader != null)
            {
                if (clickedHeader.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (clickedHeader != lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }
                    string sortString;

                    if (clickedHeader.Column.DisplayMemberBinding == null)
                        return;

                    sortString = ((Binding)clickedHeader.Column.DisplayMemberBinding).Path.Path;

                    Sort(sortString, direction);

                    lastHeaderClicked = clickedHeader;
                    lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.ItemsSource != null ? this.ItemsSource : this.Items);

            dataView.SortDescriptions.Clear();
            SortDescription sD = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sD);
            dataView.Refresh();
        }
    }
}

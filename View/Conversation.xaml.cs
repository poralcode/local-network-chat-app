using local_network_chat_app.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace local_network_chat_app.View
{
    /// <summary>
    /// Interaction logic for Conversation.xaml
    /// </summary>
    public partial class Conversation : UserControl
    {
        private bool _autoScroll = true;
        public Conversation()
        {
            InitializeComponent();
            ChatViewModel? viewModel = DataContext as ChatViewModel;
            if (viewModel != null)
            {
                viewModel.Messages.CollectionChanged += Messages_CollectionChanged;
            }

            var scrollViewer = GetScrollViewer(MessageList);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }
        private static ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer) return depObj as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            _autoScroll = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight;
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Get the ScrollViewer inside the ListBox
            var scrollViewer = GetScrollViewer(MessageList);

            // Check if the ScrollViewer is already at the bottom
            bool isAtBottom = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight;

            // If the ScrollViewer is at the bottom or this is the first message, scroll to the bottom
            if (isAtBottom || MessageList.Items.Count == 1)
            {
                ScrollToBottom();
            }
        }

        private void ScrollToBottom()
        {
            if (MessageList.Items.Count > 0)
            {
                var lastItem = MessageList.Items[MessageList.Items.Count - 1];
                MessageList.ScrollIntoView(lastItem);
            }
        }

        private void MessageList_Loaded(object sender, RoutedEventArgs e)
        {
            if (MessageList.Items.Count > 0)
            {
                var lastItem = MessageList.Items[MessageList.Items.Count - 1];
                MessageList.ScrollIntoView(lastItem);
            }
        }

    }
}

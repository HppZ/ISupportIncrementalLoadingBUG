using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ISupportIncrementalLoadingBUG
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IIncrementalSource<string>
    {
        public MainPage()
        {
            this.InitializeComponent();

            ListViewElement.ItemsSource = new List<IncrementalLoadingObservableCollection<string>>()
            {
                new IncrementalLoadingObservableCollection<string>(this),
            };
        }

        public bool HasMoreItems { get; set; } = true; // ignore

        public async Task<IEnumerable<string>> GetItems(CancellationToken token, uint count)
        {
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                list.Add(i.ToString());
            }
            return await Task.FromResult(list);
        }
    }

    public interface IIncrementalSource<T>
    {
        bool HasMoreItems { get; }
        Task<IEnumerable<T>> GetItems(CancellationToken token, uint count);
    }

    public class IncrementalLoadingObservableCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        private readonly IIncrementalSource<T> _incrementalSource;

        public IncrementalLoadingObservableCollection(IIncrementalSource<T> source)
        {
            _incrementalSource = source;
        }

        public bool HasMoreItems => this.Count <= 100;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            Debug.WriteLine($"LoadMoreItemsAsync {count}");
            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            var items = (await _incrementalSource.GetItems(c, count)).ToList();
            foreach (T item in items)
            {
                this.Add(item);
            }
            return new LoadMoreItemsResult { Count = (uint)items.Count() };
        }

    }
}

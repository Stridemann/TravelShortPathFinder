using System.Windows;

namespace TravelShortPathFinder.Gui.Views
{
	using System.Windows.Media;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			RenderOptions.SetBitmapScalingMode(Image1, BitmapScalingMode.NearestNeighbor);
		}
	}
}

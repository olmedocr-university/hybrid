using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Services.Maps;
using Windows.UI;
using Windows.UI.Popups;

namespace MyMap
{
    public sealed partial class MainPage : Page
    {
        Geopoint currentLocation;

        public MainPage()
        {
            this.InitializeComponent();

            LoadLocation();
        }

        private async void LoadLocation()
        {
            if (await Geolocator.RequestAccessAsync() == GeolocationAccessStatus.Allowed)
            {
                var MyLandmarks = new List<MapElement>();

                Geolocator geolocator = new Geolocator();
                Geoposition pos = await geolocator.GetGeopositionAsync();
                Geopoint myLocation = pos.Coordinate.Point;

                currentLocation = myLocation;

                var icon = new MapIcon
                {
                    Location = myLocation,
                    ZIndex = 0
                };

                MyLandmarks.Add(icon);

                var LandmarksLayer = new MapElementsLayer
                {
                    ZIndex = 1,
                    MapElements = MyLandmarks
                };

                MyMap.Center = myLocation;
                MyMap.ZoomLevel = 14;
                MyMap.LandmarksVisible = true;
                MyMap.Layers.Add(LandmarksLayer);
            }
        }

        private async void GoButton_Click(Object sender, RoutedEventArgs e)
        {
            string originAddressToGeocode = OriginTextBox.Text;
            string destinationAddressToGeocode = DestinationTextBox.Text;

            MapLocationFinderResult originLocationResult = await MapLocationFinder.FindLocationsAsync(
                                   originAddressToGeocode,
                                   currentLocation,
                                   1);

            MapLocationFinderResult destinationLocationResult = await MapLocationFinder.FindLocationsAsync(
                                   destinationAddressToGeocode,
                                   currentLocation,
                                   1);


            Geopoint origin = null;
            Geopoint destination = null;

            if (OriginTextBox.Text != "")
            { 
                if (originLocationResult.Status == MapLocationFinderStatus.Success)
                {
                    origin = originLocationResult.Locations[0].Point;
                }
                else
                {
                    showAlert();
                    return;
                }
            }
            else
            {
                origin = currentLocation;
            }

            if  (destinationLocationResult.Status == MapLocationFinderStatus.Success && DestinationTextBox.Text != "")
            {
                destination = destinationLocationResult.Locations[0].Point;
            }
            else
            {
                showAlert();
                return;
            }

            MapRouteFinderResult routeResult =
                await MapRouteFinder.GetDrivingRouteAsync(
                origin,
                destination,
                MapRouteOptimization.Time,
                MapRouteRestrictions.None);

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Orange;
                viewOfRoute.OutlineColor = Colors.White;

                MyMap.Routes.Clear();
                MyMap.Routes.Add(viewOfRoute);

                await MyMap.TrySetViewBoundsAsync(
                      routeResult.Route.BoundingBox,
                      null,
                      MapAnimationKind.None);
            }
                // FIXME: return more than one option and let the user select the destination

        }

        private async void showAlert()
        {
            MessageDialog dialog = new MessageDialog("Could not find the route, make sure both fields are filled correctly");
            dialog.Commands.Add(new UICommand("OK", null));
            dialog.CancelCommandIndex = 0;
            var cmd = await dialog.ShowAsync();

        }

    }
}

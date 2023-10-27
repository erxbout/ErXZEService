using Xamarin.Forms;

namespace ErXZEService.ViewModels
{
    public class ChargingStationsViewModel : BaseViewModel
    {
        #region Properties
        
        public HtmlWebViewSource HtmlSource { get; set; }
        #endregion
        
        public ChargingStationsViewModel()
        {   
            Title = "ChargingStations";
            
            string widgetHtml = "<!DOCTYPE HTML><html><body>";
            widgetHtml += "<!-- Widgetcode, ab hier nicht mehr verändern --> <div id=\"goingelectric - widget\"><script src=\"//www.goingelectric.de/stromtankstellen/widget/widget.php?search=top&searchoptions=true&backgroundcolor=f0f0f0&textcolor=000000&buttontextcolor=FFFFFF&buttoncolor=3382BE&clustering=true&lat=47.41322033016904&lon=12.523408910309682&zoom=6&id=&verbund=alle&stecker=typ2,ceerot,ceeblau,schuko,\"></script><div id=\"widget-container\" name=\"widget-container\"></div><div id=\"widget-info\" name=\"widget-info\" style=\"color:#000000;background-color:#f0f0f0;\">Stromtankstellen-Widget von <a id=\"widget-info-link\" href=\"http://www.goingelectric.de/\" target=\"_blank\" style=\"color:#000000;margin-right:3px;\">GoingElectric.de</a></div></div> <!-- Ende Widgetcode -->";
            widgetHtml += "</body></html>";

            var htmlSource = new HtmlWebViewSource();
            //htmlSource.Html = widgetHtml;
            htmlSource.BaseUrl = "https://erxbout.at";

            HtmlSource = htmlSource;
            PropChanged(nameof(HtmlSource));
        }
    }
}
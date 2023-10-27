using Xamarin.Forms;

namespace ErXZEService.Models
{
    public class TopicModelItem
    {
        public int Index { get; set; }

        private string _caption;
        public string Caption
        {
            get
            {
                if (ItemInstance is ITopicModelItem t && _caption == null)
                    return t.Caption;

                return _caption;
            }
            set
            {
                _caption = value;
            }
        }

        private string _subCaption;
        public string SubCaption
        {
            get
            {
                if (ItemInstance is ITopicModelItem t && _subCaption == null)
                    return t.SubCaption;

                return _subCaption;
            }
            set
            {
                _subCaption = value;
            }
        }

        private string _monthCaption;
        public string MonthCaption
        {
            get
            {
                if (ItemInstance is ITopicModelItem t && _monthCaption == null)
                    return t.MonthCaption;

                return _monthCaption;
            }
            set
            {
                _monthCaption = value;
            }
        }

        public int PageinationSetIndex { get; set; }

        public string ImageSource { get; set; }

        public bool UseImageSource => !string.IsNullOrEmpty(ImageSource);
        public bool UseDrawnBattery => string.IsNullOrEmpty(ImageSource);

        public Color FillColor { get; set; }
        public int FillPercent { get; set; }

        public object ItemInstance { get; set; }
    }
}
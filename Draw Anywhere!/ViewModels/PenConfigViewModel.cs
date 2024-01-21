using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using DrawAnywhere.MvvmCore;

namespace DrawAnywhere.ViewModels
{
    internal class PenConfigViewModel : ObservableObject
    {
        public PenConfigViewModel(DrawingAttributes boundAttributes)
        {
            _boundAttributes = boundAttributes;
            StrokeHeight = (int)boundAttributes.Height;
            StrokeWidth = (int)boundAttributes.Width;
            IsHighlighter = boundAttributes.IsHighlighter;
        }

		public int StrokeHeight
		{
			get => (int)_strokeHeight;
            set
            {
                _strokeHeight = value;
                _boundAttributes.Height = value;
                OnPropertyChanged();
            }
		}

		public int StrokeWidth
		{
			get => (int)_strokeWidth;
            set
            {
                _strokeWidth = value;
                _boundAttributes.Width = value;
                OnPropertyChanged();
            }
		}

		public bool IsHighlighter
		{
			get => _isHighlighter;
            set
            {
                _isHighlighter = value;
                _boundAttributes.IsHighlighter = value;
                OnPropertyChanged();
            }
		}

        private readonly DrawingAttributes _boundAttributes;
        private double _strokeHeight;
        private double _strokeWidth;
        private bool _isHighlighter;
	}
}

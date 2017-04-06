using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace NPLTools.Language.Tooltip
{
    internal class NPLQuickInfoController : IIntellisenseController
    {
        private ITextView _textView;
        private IList<ITextBuffer> _subjectBuffers;
        private NPLQuickInfoControllerProvider _provider;
        private IQuickInfoSession _session;

        internal NPLQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, NPLQuickInfoControllerProvider provider)
        {
            _textView = textView;
            _subjectBuffers = subjectBuffers;
            _provider = provider;

            _textView.MouseHover += this.OnTextViewMouseHover;
        }

        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            SnapshotPoint? point = _textView.BufferGraph.MapDownToFirstMatch
                (new SnapshotPoint(_textView.TextSnapshot, e.Position),
                PointTrackingMode.Positive,
                snapshot => _subjectBuffers.Contains(snapshot.TextBuffer),
                PositionAffinity.Predecessor);

            if (point != null)
            {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive);

                if (!_provider.QuickInfoBroker.IsQuickInfoActive(_textView))
                {
                    _session = _provider.QuickInfoBroker.TriggerQuickInfo(_textView, triggerPoint, true);
                }
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            
        }

        public void Detach(ITextView textView)
        {
            if (_textView == textView)
            {
                _textView.MouseHover -= this.OnTextViewMouseHover;
                _textView = null;
            }
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            
        }
    }

    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("ToolTip QuickInfo Controller")]
    [ContentType("NPL")]
    internal class NPLQuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        internal IQuickInfoBroker QuickInfoBroker { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            return new NPLQuickInfoController(textView, subjectBuffers, this);
        }
    }
}

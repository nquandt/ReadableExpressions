﻿namespace AgileObjects.ReadableExpressions.Visualizers
{
    using Dialog;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class Vs17ExpressionVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(
            IDialogVisualizerService windowService,
            IVisualizerObjectProvider objectProvider)
        {
            using (var dialog = new VisualizerDialog(objectProvider.GetObject))
            {
                windowService.ShowDialog(dialog);
            }
        }
    }
}

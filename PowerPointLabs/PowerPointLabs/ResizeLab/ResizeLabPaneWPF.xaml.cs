﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PowerPointLabs.ActionFramework.Common.Extension;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace PowerPointLabs.ResizeLab
{
    /// <summary>
    /// Interaction logic for ResizePane.xaml
    /// </summary>
    public partial class ResizeLabPaneWPF : IResizeLabPane
    {
        private ResizeLabMain _resizeLab;
        private readonly ResizeLabErrorHandler _errorHandler;
        private Dictionary<ResizeLabMain.AnchorPoint, RadioButton> _anchorButtonLookUp;
        public static bool IsAspectRatioLocked { get; set; }
        
        private const string UnlockAspectRatioToolTip = "Unlocks the aspect ratio of objects when performing resizing of objects";
        private const string LockAspectRatioToolTip = "Locks the aspect ratio of objects when performing resizing of objects";

        // Dialog windows
        private StretchSettingsDialog _stretchSettingsDialog;
        private AdjustProportionallySettingsDialog _adjustProportionallySettingsDialog;
        private SlightAdjustSettingsDialog _slightAdjustSettingsDialog;

        // For preview
        private const int PreviewDelay = 500;
        private DispatcherTimer _previewTimer;
        private delegate void PreviewCallBack();
        private PreviewCallBack _previewCallBack;

        public ResizeLabPaneWPF()
        {
            InitializeComponent();
            InitialiseLogicInstance();
            InitialiseAnchorButton();
            InitialiseTimer();
            _errorHandler = ResizeLabErrorHandler.InitializErrorHandler(this);
            UnlockAspectRatio();
        }

        #region Initialise
        internal void InitialiseLogicInstance()
        {
            if (_resizeLab == null)
            {
                _resizeLab = new ResizeLabMain();
            }
        }

        private void InitialiseAnchorButton()
        {
            _anchorButtonLookUp = new Dictionary<ResizeLabMain.AnchorPoint, RadioButton>()
            {
                { ResizeLabMain.AnchorPoint.TopLeft, AnchorTopLeftBtn},
                { ResizeLabMain.AnchorPoint.TopCenter, AnchorTopMidBtn},
                { ResizeLabMain.AnchorPoint.TopRight, AnchorTopRightBtn},
                { ResizeLabMain.AnchorPoint.MiddleLeft, AnchorMidLeftBtn},
                { ResizeLabMain.AnchorPoint.Center, AnchorMidBtn},
                { ResizeLabMain.AnchorPoint.MiddleRight, AnchorMidRightBtn},
                { ResizeLabMain.AnchorPoint.BottomLeft, AnchorBottomLeftBtn},
                { ResizeLabMain.AnchorPoint.BottomCenter, AnchorBottomMidBtn},
                { ResizeLabMain.AnchorPoint.BottomRight, AnchorBottomRightBtn}
            };
            AnchorTopLeftBtn.IsChecked = true;
        }

        private void InitialiseTimer()
        {
            _previewTimer = new DispatcherTimer();
            _previewTimer.Tick += Timer_Tick;
            _previewTimer.Interval = new TimeSpan(0, 0, 0, 0, PreviewDelay);
        }

        #endregion

        #region Execute Stretch and Shrink

        private void StretchLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.StretchLeft(shapes);
            ClickHandler(resizeAction, ResizeLabMain.Stretch_MinNoOfShapesRequired,
                ResizeLabMain.Stretch_ErrorParameters);
        }

        private void StretchRightBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.StretchRight(shapes);
            ClickHandler(resizeAction, ResizeLabMain.Stretch_MinNoOfShapesRequired,
                ResizeLabMain.Stretch_ErrorParameters);
        }

        private void StretchTopBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.StretchTop(shapes);
            ClickHandler(resizeAction, ResizeLabMain.Stretch_MinNoOfShapesRequired,
                ResizeLabMain.Stretch_ErrorParameters);
        }

        private void StretchBottomBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.StretchBottom(shapes);
            ClickHandler(resizeAction, ResizeLabMain.Stretch_MinNoOfShapesRequired,
                ResizeLabMain.Stretch_ErrorParameters);
        }

        private void StretchSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_stretchSettingsDialog == null || !_stretchSettingsDialog.IsOpen)
            {
                _stretchSettingsDialog = new StretchSettingsDialog(_resizeLab);
                _stretchSettingsDialog.Show();
            }
            else
            {
                _stretchSettingsDialog.Activate();
            }
            
        }

        #endregion

        #region Execute Same Dimension

        private void SameWidthBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.ResizeToSameWidth(shapes);
            ClickHandler(resizeAction, ResizeLabMain.Equalize_MinNoOfShapesRequired,
                ResizeLabMain.Equalize_ErrorParameters);
        }

        private void SameHeightBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.ResizeToSameHeight(shapes);
            ClickHandler(resizeAction, ResizeLabMain.Equalize_MinNoOfShapesRequired,
                            ResizeLabMain.Equalize_ErrorParameters);
        }

        private void SameSizeBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.ResizeToSameHeightAndWidth(shapes);
            ClickHandler(resizeAction, ResizeLabMain.Equalize_MinNoOfShapesRequired,
                ResizeLabMain.Equalize_ErrorParameters);
        }

        #endregion

        #region Execute Fit
        private void FitWidthBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange, float, float, bool> resizeAction =
                (shapes, referenceWidth, referenceHeight, isAspectRatio) =>
                {
                    _resizeLab.FitToWidth(shapes, referenceWidth, referenceHeight, isAspectRatio);
                };
            ClickHandler(resizeAction, ResizeLabMain.Fit_MinNoOfShapesRequired,
                ResizeLabMain.Fit_ErrorParameters);
        }

        private void FitHeightBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange, float, float, bool> resizeAction =
                (shapes, referenceWidth, referenceHeight, isAspectRatio) =>
                {
                    _resizeLab.FitToHeight(shapes, referenceWidth, referenceHeight, isAspectRatio);
                };
            ClickHandler(resizeAction, ResizeLabMain.Fit_MinNoOfShapesRequired,
                            ResizeLabMain.Fit_ErrorParameters);
        }

        private void FillBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange, float, float, bool> resizeAction =
                (shapes, referenceWidth, referenceHeight, isAspectRatio) =>
                {
                    _resizeLab.FitToFill(shapes, referenceWidth, referenceHeight, isAspectRatio);
                };
            ClickHandler(resizeAction, ResizeLabMain.Fit_MinNoOfShapesRequired,
                ResizeLabMain.Fit_ErrorParameters);
        }

        #endregion

        #region Execute Slight Adjust
        private void IncreaseHeightBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.IncreaseHeight(shapes);
            ClickHandler(resizeAction, ResizeLabMain.SlightAdjust_MinNoOfShapesRequired,
                ResizeLabMain.SlightAdjust_ErrorParameters);
        }

        private void DecreaseHeightBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.DecreaseHeight(shapes);
            ClickHandler(resizeAction, ResizeLabMain.SlightAdjust_MinNoOfShapesRequired,
                ResizeLabMain.SlightAdjust_ErrorParameters);
        }

        private void IncreaseWidthBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.IncreaseWidth(shapes);
            ClickHandler(resizeAction, ResizeLabMain.SlightAdjust_MinNoOfShapesRequired,
                ResizeLabMain.SlightAdjust_ErrorParameters);
        }

        private void DecreaseWidthBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.DecreaseWidth(shapes);
            ClickHandler(resizeAction, ResizeLabMain.SlightAdjust_MinNoOfShapesRequired,
                ResizeLabMain.SlightAdjust_ErrorParameters);
        }

        private void SlightAdjustSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_slightAdjustSettingsDialog == null || !_slightAdjustSettingsDialog.IsOpen)
            {
                _slightAdjustSettingsDialog = new SlightAdjustSettingsDialog(_resizeLab);
                _slightAdjustSettingsDialog.Show();
            }
            else
            {
                _slightAdjustSettingsDialog.Activate();
            }
        }

        #endregion

        #region Execute Match
        private void MatchWidthBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.MatchWidth(shapes);
            ClickHandler(resizeAction, ResizeLabMain.Match_MinNoOfShapesRequired,
                ResizeLabMain.Match_ErrorParameters);
        }

        private void MatchHeightBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.MatchHeight(shapes);
            ClickHandler(resizeAction, ResizeLabMain.Match_MinNoOfShapesRequired,
                ResizeLabMain.Match_ErrorParameters);
        }

        #endregion

        #region Execute Adjust Proportionally
        private void ProportionalWidthBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.AdjustWidthProportionally(shapes);
            ClickHandler(resizeAction, ResizeLabMain.AdjustProportionally_MinNoOfShapesRequired,
                ResizeLabMain.AdjustProportionally_ErrorParameters);
        }

        private void ProportionalHeightBtn_Click(object sender, RoutedEventArgs e)
        {
            Action<PowerPoint.ShapeRange> resizeAction = shapes => _resizeLab.AdjustHeightProportionally(shapes);
            ClickHandler(resizeAction, ResizeLabMain.AdjustProportionally_MinNoOfShapesRequired,
                ResizeLabMain.AdjustProportionally_ErrorParameters);
        }

        private void ProportionalSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_adjustProportionallySettingsDialog == null || !_adjustProportionallySettingsDialog.IsOpen)
            {
                _adjustProportionallySettingsDialog = new AdjustProportionallySettingsDialog(_resizeLab);
                _adjustProportionallySettingsDialog.Show();
            }
            else
            {
                _adjustProportionallySettingsDialog.Activate();
            }

        }

        #endregion

        #region Preview Stretch and Shrink

        private void StretchLeftBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.StretchLeft(shapes);
            PreviewHandler(previewAction, ResizeLabMain.Stretch_MinNoOfShapesRequired);
        }

        private void StretchRightBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.StretchRight(shapes);
            PreviewHandler(previewAction, ResizeLabMain.Stretch_MinNoOfShapesRequired);
        }

        private void StretchTopBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.StretchTop(shapes);
            PreviewHandler(previewAction, ResizeLabMain.Stretch_MinNoOfShapesRequired);
        }

        private void StretchBottomBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.StretchBottom(shapes);
            PreviewHandler(previewAction, ResizeLabMain.Stretch_MinNoOfShapesRequired);
        }

        #endregion

        #region Preview Same Dimension

        private void SameWidthBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.ResizeToSameWidth(shapes);
            PreviewHandler(previewAction, ResizeLabMain.Equalize_MinNoOfShapesRequired);
        }
        
        private void SameHeightBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.ResizeToSameHeight(shapes);
            PreviewHandler(previewAction, ResizeLabMain.Equalize_MinNoOfShapesRequired);
        }

        private void SameSizeBtn_MouseEnter(object sender, MouseEventArgs e)
        { 
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.ResizeToSameHeightAndWidth(shapes);

            PreviewHandler(previewAction, ResizeLabMain.Equalize_MinNoOfShapesRequired);
        }

        #endregion

        #region Preview Fit

        private void FitWidthBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange, float, float, bool> previewAction =
                (shapes, referenceWidth, referenceHeight, isAspectRatio) =>
                {
                    _resizeLab.FitToWidth(shapes, referenceWidth, referenceHeight, isAspectRatio);
                };

            PreviewHandler(previewAction);
        }

        private void FitHeightBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange, float, float, bool> previewAction =
                (shapes, referenceWidth, referenceHeight, isAspectRatio) =>
                {
                    _resizeLab.FitToHeight(shapes, referenceWidth, referenceHeight, isAspectRatio);
                };
            PreviewHandler(previewAction);
        }

        private void FillBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange, float, float, bool> previewAction =
                (shapes, referenceWidth, referenceHeight, isAspectRatio) =>
                {
                    _resizeLab.FitToFill(shapes, referenceWidth, referenceHeight, isAspectRatio);
                };
            PreviewHandler(previewAction);
        }

        #endregion

        #region Preview Slight Adjust
        private void IncreaseHeightBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.IncreaseHeight(shapes);
            PreviewHandler(previewAction, ResizeLabMain.SlightAdjust_MinNoOfShapesRequired);
        }

        private void DecreaseHeightBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.DecreaseHeight(shapes);
            PreviewHandler(previewAction, ResizeLabMain.SlightAdjust_MinNoOfShapesRequired);
        }

        private void IncreaseWidthBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.IncreaseWidth(shapes);
            PreviewHandler(previewAction, ResizeLabMain.SlightAdjust_MinNoOfShapesRequired);
        }

        private void DecreaseWidthBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.DecreaseWidth(shapes);
            PreviewHandler(previewAction, ResizeLabMain.SlightAdjust_MinNoOfShapesRequired);
        }

        #endregion

        #region Preview Match
        private void MatchWidthBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.MatchWidth(shapes);
            PreviewHandler(previewAction, ResizeLabMain.Match_MinNoOfShapesRequired);
        }

        private void MatchHeightBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.MatchHeight(shapes);
            PreviewHandler(previewAction, ResizeLabMain.Match_MinNoOfShapesRequired);
        }

        #endregion

        #region Preview Adjust Proportionally
        private void ProportionalWidthBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.AdjustWidthProportionally(shapes);
            PreviewHandler(previewAction, ResizeLabMain.AdjustProportionally_MinNoOfShapesRequired);
        }

        private void ProportionalHeightBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Action<PowerPoint.ShapeRange> previewAction = shapes => _resizeLab.AdjustHeightProportionally(shapes);
            PreviewHandler(previewAction, ResizeLabMain.AdjustProportionally_MinNoOfShapesRequired);
        }

        #endregion

        #region Main Settings

        private void LockAspectRatio_UnChecked(object sender, RoutedEventArgs e)
        {
            UnlockAspectRatio();
        }

        private void LockAspectRatio_Checked(object sender, RoutedEventArgs e)
        {
            LockAspectRatio();
        }

        private void UnlockAspectRatio()
        {
            IsAspectRatioLocked = false;
            LockAspectRatioCheckBox.ToolTip = LockAspectRatioToolTip;

            ModifySelectionAspectRatio();
        }

        private void LockAspectRatio()
        {
            IsAspectRatioLocked = true;
            LockAspectRatioCheckBox.ToolTip = UnlockAspectRatioToolTip;

            ModifySelectionAspectRatio();
        }

        private void ModifySelectionAspectRatio()
        {
            if (_resizeLab.IsSelectionValid(GetSelection(), false))
            {
                _resizeLab.ChangeShapesAspectRatio(GetSelectedShapes(), IsAspectRatioLocked);
            }
        }

        private void AnchorBtn_Checked(object sender, RoutedEventArgs e)
        {
            var checkedButton = sender as RadioButton;
            foreach (var anAnchorPair in _anchorButtonLookUp)
            {
                if (checkedButton.Equals(anAnchorPair.Value))
                {
                    _resizeLab.AnchorPointType = anAnchorPair.Key;
                    return;
                }
            }
        }

        #endregion

        #region Miscellaneous events
        private void Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_previewTimer.IsEnabled) // Actual preview did not execute
            {
                StopTimer();
                _previewCallBack = null;
            }
            else // Preview was executed
            {
                Reset();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            StopTimer();
            if (_previewCallBack != null)
            {
                _previewCallBack.Invoke();
                _previewCallBack = null;
            }

        }

        #endregion

        #region Helper Functions

        private PowerPoint.ShapeRange GetSelectedShapes(bool handleError = false)
        {
            var selection = GetSelection();

            return _resizeLab.IsSelectionValid(selection, handleError) ? GetSelection().ShapeRange : null;
        }

        private PowerPoint.Selection GetSelection()
        {
            return this.GetCurrentSelection();
        }

        private void ClickHandler(Action<PowerPoint.ShapeRange> resizeAction, int minNoOfSelectedShapes, string[] errorParameters)
        {
            var selectedShapes = GetSelectedShapes();

            if (selectedShapes == null || selectedShapes.Count < minNoOfSelectedShapes)
            {
                _errorHandler.ProcessErrorCode(ResizeLabErrorHandler.ErrorCodeInvalidSelection, errorParameters);
                return;
            }

            ModifySelectionAspectRatio();
            ExecuteResizeAction(selectedShapes, resizeAction);
        }

        private void ClickHandler(Action<PowerPoint.ShapeRange, float, float, bool> resizeAction, int minNoOfSelectedShapes,
            string[] errorParameters)
        {
            var selectedShapes = GetSelectedShapes();
            var slideWidth = this.GetCurrentPresentation().SlideWidth;
            var slideHeight = this.GetCurrentPresentation().SlideHeight;

            if (selectedShapes == null || selectedShapes.Count < minNoOfSelectedShapes)
            {
                _errorHandler.ProcessErrorCode(ResizeLabErrorHandler.ErrorCodeInvalidSelection, errorParameters);
                return;
            }

            ModifySelectionAspectRatio();
            ExecuteResizeAction(selectedShapes, slideWidth, slideHeight, resizeAction);
        }

        private void PreviewHandler(Action<PowerPoint.ShapeRange> previewAction, int minNoOfSelectedShapes)
        {
            _previewCallBack = delegate
            {
                var selectedShapes = GetSelectedShapes();

                ModifySelectionAspectRatio();
                Preview(selectedShapes, previewAction, minNoOfSelectedShapes);
            };
            StartTimer();
        }

        private void PreviewHandler(Action<PowerPoint.ShapeRange, float, float, bool> previewAction)
        {
            _previewCallBack = delegate
            {
                var selectedShapes = GetSelectedShapes();
                var slideWidth = this.GetCurrentPresentation().SlideWidth;
                var slideHeight = this.GetCurrentPresentation().SlideHeight;

                ModifySelectionAspectRatio();
                Preview(selectedShapes, slideWidth, slideHeight, previewAction);
            };
            StartTimer();
        }

        private void StopTimer()
        {
            _previewTimer.Stop();
            _previewTimer.IsEnabled = false;
        }

        private void StartTimer()
        {
            _previewTimer.IsEnabled = true;
            _previewTimer.Start();
        }

        #endregion
    }
}
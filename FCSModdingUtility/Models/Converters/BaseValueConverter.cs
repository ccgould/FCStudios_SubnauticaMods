using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace FCSModdingUtility
{
    /// <summary>
    /// A base value converter that allows direct XAML usage
    /// </summary>
    /// <typeparam name="T">The type of this value converter</typeparam>
    public abstract class BaseValueConverter<T> : MarkupExtension, IValueConverter
    where T : class, new()
    {
        #region Private Members
        /// <summary>
        /// A single instance of this value converter
        /// </summary>
        private static T _mConverter;
        #endregion

        #region Markup Extension Methods

        /// <summary>
        /// Provides a static instance of the value converter
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            #region Long way of doing below

            //if(mConverter == null)
            //    mConverter = new T();
            //return mConverter;

            #endregion
            return _mConverter ?? (_mConverter = new T());
        }

        #region Value Converter Methods

        /// <summary>
        /// The method to convert one type to another
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        /// <summary>
        /// The method to convert a value back yo its source type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);


        #endregion


        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffe_Grinder
{
    public class CoffeeItem : INotifyPropertyChanged
    {
        public int CoffeeID { get; set; }
        public string CoffeeName { get; set; }
        public CoffeeType CoffeeType { get; set; }
        public decimal PricePerKg { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Subtotal => Quantity * PricePerKg;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

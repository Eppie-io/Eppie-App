using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Tuvi.App.ViewModels.Validation;

namespace Tuvi.App.ViewModels.Common
{
    public class SeedPhraseModel : List<SeedWordModel>, INotifyPropertyChanged
    {
        public SeedPhraseModel(int wordsCount)
            : base()
        {
            for (int i = 1; i <= wordsCount; i++)
            {
                AddWordModel(new SeedWordModel(
                    number: i,
                    word: string.Empty,
                    isReadOnly: false));
            }
        }

        public SeedPhraseModel(string[] seed, bool isReadOnly)
            : base()
        {
            if (seed == null)
            {
                throw new ArgumentNullException(nameof(seed));
            }

            AddWords(seed, isReadOnly);
        }

        private void AddWords(string[] seed, bool isReadOnly)
        {
            for (int i = 0; i < seed.Length; i++)
            {
                AddWordModel(new SeedWordModel(
                    number: i + 1,
                    word: seed[i],
                    isReadOnly: isReadOnly));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddWordModel(SeedWordModel wordModel)
        {
            wordModel.PropertyChanged += (sender, args) => PropertyChanged?.Invoke(this, args);
            Add(wordModel);
        }

        public string[] GetPhrase()
        {
            return this.Select(element => element.Word.Value ?? string.Empty).ToArray();
        }

        private const char ImportedSeedSeparator = ' ';

        private string[] PrepareImportedSeed(string importedSeed)
        {
            int seedLength = Count;
            var seedWords = new string[seedLength];
            importedSeed.Split(ImportedSeedSeparator)
                        .Where(word => !String.IsNullOrEmpty(word))
                        .Take(seedLength)
                        .ToArray()
                        .CopyTo(seedWords, 0);
            return seedWords;
        }

        private readonly string ExportedSeedSeparator = " ";

        public string CleanText 
        {
            get { return string.Join(ExportedSeedSeparator, GetPhrase()).Trim(); }
            set
            {
                Import(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CleanText)));
            }
        }

        public void Import(string value)
        {
            var seedWords = PrepareImportedSeed(value);
            Clear();
            AddWords(seedWords, false);
        }
    }

    public class SeedWordModel : ObservableObject
    {
        public SeedWordModel(int number, string word, bool isReadOnly)
        {
            Number = number;
            Word.SetInitialValue(word);
            Word.NeedsValidation = !isReadOnly;
            IsReadOnly = isReadOnly;

            Word.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ValidatableProperty<string>.Value))
                {
                    WasChecked = false;
                    OnPropertyChanged(nameof(Word));
                }
                else if (args.PropertyName == nameof(ValidatableProperty<string>.NeedsValidation))
                {
                    OnPropertyChanged(nameof(Word));
                }
            };
        }

        /// <summary>
        /// Shows whether Word value ran through check
        /// </summary>
        public bool WasChecked { get; set; }

        public int Number { get; set; }

        public bool IsReadOnly { get; set; }

        public ValidatableProperty<string> Word { get; } = new ValidatableProperty<string>();
    }
}

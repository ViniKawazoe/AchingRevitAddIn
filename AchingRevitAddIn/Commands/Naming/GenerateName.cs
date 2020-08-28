namespace AchingRevitAddIn
{
    /// <summary>
    /// Generate the name of the element
    /// Join a prefix with the number of the element
    /// The number of the element has to be defined BEFORE using this class
    /// </summary>
    public class GenerateName
    {
        public int ItemNumber { get; set; }
        public string Prefix { get; set; }
        public int NumberOfItems { get; set; }

        #region constructors
        public GenerateName()
        {
        }

        public GenerateName(int itemNumber, string prefix, int numberOfItems)
        {
            ItemNumber = itemNumber;
            Prefix = prefix;
            NumberOfItems = numberOfItems;
        }
        #endregion

        #region public methods
        public string Name(int itemNumber, string prefix, int numberOfItems)
        {
            string name = "";

            if(numberOfItems < 10)
            {
                name = prefix + itemNumber;
            }
            else if(numberOfItems >= 10 && numberOfItems < 100)
            {
                if(itemNumber < 10)
                {
                    name = prefix + "0" + itemNumber;
                }
                else
                {
                    name = prefix + itemNumber;
                }
            }
            else if(numberOfItems >= 100 && numberOfItems < 1000)
            {
                if(itemNumber < 10)
                {
                    name = prefix + "00" + itemNumber;
                }
                else if(itemNumber >= 10 && itemNumber < 100)
                {
                    name = prefix + "0" + itemNumber;
                }
                else
                {
                    name = prefix + itemNumber;
                }
            }
            else
            {
                if(itemNumber < 10)
                {
                    name = prefix + "000" + itemNumber;
                }
                else if(itemNumber >= 10 && itemNumber < 100)
                {
                    name = prefix + "00" + itemNumber;
                }
                else if(itemNumber >= 100 && itemNumber < 1000)
                {
                    name = prefix + "0" + itemNumber;
                }
                else
                {
                    name = prefix + itemNumber;
                }
            }
            return name;
        }
        #endregion
    }
}

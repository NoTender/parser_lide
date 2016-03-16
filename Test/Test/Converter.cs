using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;



namespace Test

{


    public class Converter
    {
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="; //Znaky, pomoci nichz je zprava zakodovana
        private int[] asociatedAlphabet; //Pole, ktere na pozici hodnoty ASCII daneho znaku ulozi index, na kterem se znak vyskytuje.

        public Converter() //Konstruktor, ktery naplni pole asociatedAlphabet
        {
            asociatedAlphabet = new int[130];
            for (int i = 0; i < alphabet.Length; i++)
            {
                asociatedAlphabet[alphabet[i]] = i;
            }
        }

        public byte[] atob(string data)
        {
            List<byte> output = new List<byte>();
            int chr1, chr2, chr3, enc1, enc2, enc3, enc4;
            data = Regex.Replace(data, @"\s+", "");
            int i = 0;

            while (i < data.Length)
            {
                enc1 = asociatedAlphabet[data[i]];
                enc2 = asociatedAlphabet[data[i + 1]];
                enc3 = asociatedAlphabet[data[i + 2]];
                enc4 = asociatedAlphabet[data[i + 3]];

                chr1 = ((enc1 << 2) | (enc2 >> 4));
                chr2 = (((enc2 & 15) << 4) | (enc3 >> 2));
                chr3 = (((enc3 & 3) << 6) | enc4);

                output.Add((byte)chr1);
                if (enc3 != 64) { output.Add((byte)chr2); }
                if (enc4 != 64) { output.Add((byte)chr3); }

                i += 4;
            }

            return output.ToArray();
        }

    }
}   
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public static class SortExtension
{
    public static int Alphabetical(string x, string y)
    {
        if (x == y)
            return 0;

        int max = x.Length > y.Length ? x.Length : y.Length;

        for (int i = 0; i < max; i++)
        {
            if (x.Length <= i)
                return -1;

            if (y.Length <= i)
                return 1;

            if (x[i] < y[i])
                return -1;

            if (x[i] > y[i])
                return 1;
        }

        return 0;
    }


    public static void QuickSort<T>(this IList<T> list, Comparison<T> comparer)
    {
        int partition(int low, int high)
        {
            T pivot = list[high];

            int i = low - 1;

            for (int j = low; j <= high - 1; j++)
            {
                if (comparer(list[j], pivot) < 0)
                {
                    i++;
                    (list[j], list[i]) = (list[i], list[j]);
                }
            }

            (list[high], list[i + 1]) = (list[i + 1], list[high]);
            return i + 1;
        }

        void quickSort(int low, int high)
        {
            if (low < high)
            {
                int part = partition(low, high);
                quickSort(low, part - 1);
                quickSort(part + 1, high);
            }
        }

        quickSort(0, list.Count - 1);
    }


    // Main function to do heap sort
    public static void HeapSort<T>(this IList<T> list, Comparison<T> comparison)
    {
        void heapify(int n, int i)
        {
            // Initialize largest as root
            int largest = i;

            // left index = 2*i + 1
            int l = 2 * i + 1;

            // right index = 2*i + 2
            int r = 2 * i + 2;

            // If left child is larger than root
            if (l < n && comparison(list[l], list[largest]) > 0)
                largest = l;

            // If right child is larger than largest so far
            if (r < n && comparison(list[r], list[largest]) > 0)
                largest = r;

            // If largest is not root
            if (largest != i)
            {
                (list[i], list[largest]) = (list[largest], list[i]);

                // Recursively heapify the affected sub-tree
                heapify(n, largest);
            }
        }

        int n = list.Count;

        // Build heap (rearrange array)
        for (int i = n / 2 - 1; i >= 0; i--)
            heapify(n, i);

        // One by one extract an element from heap
        for (int i = n - 1; i > 0; i--)
        {
            (list[0], list[i]) = (list[i], list[0]);
            // Call max heapify on the reduced heap
            heapify(i, 0);
        }
    }

    public static void MergeSort<T>(this IList<T> list, Comparison<T> comparison)
    {
        // Merges two subarrays of arr[].
        // First subarray is arr[left..mid]
        // Second subarray is arr[mid+1..right]
        void merge(int left, int mid, int right)
        {
            int n1 = mid - left + 1;
            int n2 = right - mid;

            // Create temp vectors
            T[] L = new T[n1];
            T[] R = new T[n2];

            int i = 0, j = 0;
            // Copy data to temp vectors L[] and R[]
            for (i = 0; i < n1; i++)
                L[i] = list[left + i];

            for (j = 0; j < n2; j++)
                R[j] = list[mid + 1 + j];

            i = 0;
            j = 0;
            int k = left;

            // Merge the temp vectors back 
            // into arr[left..right]
            while (i < n1 && j < n2)
            {
                //L[i] <= R[j]
                if (comparison(L[i], R[j]) <= 0)
                {
                    list[k] = L[i];
                    i++;
                }
                else
                {
                    list[k] = R[j];
                    j++;
                }
                k++;
            }

            // Copy the remaining elements of L[], 
            // if there are any
            while (i < n1)
            {
                list[k] = L[i];
                i++;
                k++;
            }

            // Copy the remaining elements of R[], 
            // if there are any
            while (j < n2)
            {
                list[k] = R[j];
                j++;
                k++;
            }
        }

        // begin is for left index and end is right index
        // of the sub-array of arr to be sorted
        void mergeSort(int left, int right)
        {
            if (left >= right)
                return;

            int mid = left + (right - left) / 2;
            mergeSort(left, mid);
            mergeSort(mid + 1, right);
            merge(left, mid, right);
        }

        mergeSort(0, list.Count - 1);
    }
}

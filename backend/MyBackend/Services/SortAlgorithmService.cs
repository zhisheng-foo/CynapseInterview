using System;
using System.Collections.Generic;
using System.Linq;

namespace MyBackend.Services
{
    public class SortAlgorithmService
    {
        private readonly List<KeyValuePair<string, decimal>> _allocations;

        public SortAlgorithmService()
        {
            _allocations = new List<KeyValuePair<string, decimal>>();
        }

        public List<KeyValuePair<string, decimal>> Allocate(decimal totalAmount, int numberOfParticipants)
        {
            if (numberOfParticipants <= 0 || totalAmount <= 0)
                throw new ArgumentException("Invalid input values.");

            var random = new Random();
            var cutPoints = new List<decimal> { 0, totalAmount };

            // Generate random cut points
            for (int i = 0; i < numberOfParticipants - 1; i++)
            {
                cutPoints.Add((decimal)random.NextDouble() * totalAmount);
            }

            // Sort the cut points
            cutPoints.Sort();

            // Calculate differences to determine allocations
            var allocations = new List<KeyValuePair<string, decimal>>();
            decimal totalAllocated = 0;
            for (int i = 1; i < cutPoints.Count; i++)
            {
                var amount = Math.Round(cutPoints[i] - cutPoints[i - 1], 2); // Round to 2 decimals
                allocations.Add(new KeyValuePair<string, decimal>($"Participant {i}", amount));
                totalAllocated += amount;
            }

            // Adjust the last participant to make sure the total matches exactly
            var difference = totalAmount - totalAllocated;
            if (difference != 0)
            {
                // Adjust the last participant's allocation by the rounding difference
                allocations[allocations.Count - 1] = new KeyValuePair<string, decimal>(
                    allocations[allocations.Count - 1].Key,
                    allocations[allocations.Count - 1].Value + difference
                );
            }

            return allocations;
        }

        public IEnumerable<KeyValuePair<string, decimal>> GetAllAllocations()
        {
            return _allocations.AsReadOnly(); // Return all allocations
        }
    }
}

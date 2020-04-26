using System;
namespace SmashBotUltimate.Bot.Modules {

    public interface IRandomUtilitiesService {
        T PickOne<T> (params T[] candidates);
    }
    public class RandomUtilitiesService : IRandomUtilitiesService {

        private readonly Random _random;
        public RandomUtilitiesService () {
            _random = new Random ();
        }
        public T PickOne<T> (params T[] candidates) {
            return candidates[_random.Next (candidates.Length)];
        }
    }
}
using System;
namespace SmashBotUltimate.Bot.Modules {

    public interface IRandomUtilitiesService {
        T PickOne<T> (params T[] candidates);
    }
    public class RandomUtilitiesService : IRandomUtilitiesService {

        private readonly Random _random;
        public RandomUtilitiesService () {
            _random = new Random (DateTime.Today.Millisecond + DateTime.Today.Minute * 10);
        }
        public T PickOne<T> (params T[] candidates) {
            return candidates[_random.Next (candidates.Length)];
        }
    }
}
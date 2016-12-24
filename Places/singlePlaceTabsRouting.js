(function () {
    "use strict";

    angular.module(APPNAME)
        .config(["$routeProvider", "$locationProvider", function ($routeProvider, $locationProvider) {

            $routeProvider.when('/home', {
                templateUrl: '/scripts/sabio/app/singleplace/templates/home.html',
                controller: 'singlePlaceHomeTabController',
                controllerAs: 'main'
            }).when('/reviews', {
                templateUrl: '/scripts/sabio/app/singleplace/templates/reviews.html',
                controller: 'singlePlaceReviewsTabController',
                controllerAs: 'dashboard'
            }).when('/media', {
                templateUrl: '/scripts/sabio/app/singleplace/templates/media.html',
                controller: 'singlePlaceGalleryTabController',
                controllerAs: 'MediaCtrl'
            }).when('/followers', {
                templateUrl: '/scripts/sabio/app/singleplace/templates/followers.html',
                controller: 'singlePlaceFollowingPlacesTabController',
                controllerAs: 'following'
            }).otherwise('/home');

            $locationProvider.html5Mode(false);

        }]);

})();
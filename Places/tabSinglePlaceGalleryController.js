(function () {
    'use strict';
    angular.module(APPNAME)
    .controller('singlePlaceGalleryController', GalleryController);
    GalleryController.$inject = ['$scope', '$baseController', 'Lightbox', '$placesService', '$uibModal', '$mediaService', '$filter', '$voteService', '$imageService'];

    function GalleryController(
    $scope,
    $baseController,
    Lightbox,
    $placesService,
    $uibModal,
    $mediaService,
    $filter,
    $voteService,
    $imageService
    ) {

        var vm = this;
        $baseController.merge(vm, $baseController);

        vm.slug = $("#singlePlaceSlug").val();


        vm.$scope = $scope;
        vm.$placesService = $placesService;
        vm.$uibModal = $uibModal;
        vm.$mediaService = $mediaService;
        vm.$voteService = $voteService;
        vm.$filter = $filter;
        vm.$imageService = $imageService;
        vm.theplaceId = 0;
        vm.imagesVideos = [];
        vm.filterimagesVideos = [];
        vm.postVote = {}
        vm.postVote.VoteType = 4;
        vm.postVote.UserName = "Media";
        vm.postVote.isMedia = true;
        vm.CurrentPage = 1;
        vm.PerPage = 10;
        vm.totalGalleryItems = null;
        vm.getSecondIndex = [];

        vm.OpenModel = _OpenModel;
        vm.OpenVideoModel = _OpenVideoModel;

        vm.renderSuccess = _renderSuccess;
        vm.notify = vm.$placesService.getNotifier($scope);
        //vm.$systemEventService.listen("placeLoaded", _placesId);

        //vm.placesId = _placesId;
        vm.openLightboxModal = _openLightboxModal;
        vm.Lightbox = Lightbox;
        vm.loadMedia = _loadMedia;
        vm.toggleVidImg = _toggleVidImg
        vm.upVote = _upVote;
        vm.downVote = _downVote;
        vm.pageChanged = _pageChanged;

        /// ATTEMPT 2: Using Angular-Responsive
        //vm.displayMode = 'mobile'; // default value


        //vm.$watch('displayMode', function(value) {
  
        //    switch (value) {
        //        case 'mobile':
        //            // do stuff for mobile mode
        //            console.log(value);
        //            break;
        //        case 'tablet':
        //            // do stuff for tablet mode
        //            console.log(value);
        //            break;
        //    }
        //});

        //var whatDevice = vm.nowDevice;
        //vm.myInterval = 0;
        //vm.mediaSlides = [];



        _init();

        function _init() {
            vm.$placesService.getBySlug(vm.slug, _receivePlacesItems);
        }

        function _receivePlacesItems(data) {
            vm.place = data;

            vm.theplaceId = vm.place.id;
            vm.loadMedia();
        }

        function _toggleVidImg(data) {
            if (data === 1) {
                vm.filterimagesVideos = vm.$filter('filter')(vm.imagesVideos, { 'MediaType': 1 });
            }
            else if (data === 2) {

                vm.filterimagesVideos = vm.$filter('filter')(vm.imagesVideos, { 'MediaType': 2 });
            }
            else if (data === 3) {

                vm.filterimagesVideos = vm.imagesVideos;
            }
        }

        function _loadMedia() {
            vm.$placesService.getMediaByplacesId(vm.theplaceId, vm.CurrentPage, vm.PerPage, vm.renderSuccess);
        }

        function _renderSuccess(data) {
            vm.imagesVideos = [];
            vm.totalGalleryItems = data.totalItemCount;
            if (data.items && data.items.length) {

                var i = 0;
                for (var i = 0; i < data.items.length; i++) {
                    if (data.items[i].mediaType == 1) {

                        // ATTEMPT 1: make second index
                        //vm.getSecondIndex = function (i) {
                        //    if (i - data.items.length >= 0)
                        //        return i - data.items.length;
                        //    else
                        //        return index;
                        //}


                        var thegoods = {
                            //image
                            'url': data.items[i].url,
                            'thumbUrl': vm.$imageService.getImageResizeUrl(data.items[i].url, 64, 64),
                            'MediaType': 1,
                            'arryNumber': i,
                            'mediaId': data.items[i].mediaId,
                            'netVote': data.items[i].netVote,
                            'userId': data.items[i].userId,
                            'hasVoted': data.items[i].hasVoted,
                            'reviewPointScore': data.items[i].reviewPointScore,
                            'created':data.items[i].created
                        }

                        vm.imagesVideos.push(thegoods);
                    }
                    else if (data.items[i].mediaType == 2) {

                        // ATTEMPT 1: make second index
                        //vm.getSecondIndex = function (i) {
                        //    if (i - data.items.length >= 0)
                        //        return i - data.items.length;
                        //    else
                        //        return index;
                        //}


                        var thevideogoods = {
                            //video
                            'type': 'video',
                            'url': data.items[i].url,
                            'thumbUrl': vm.$imageService.getImageResizeUrl(data.items[i].thumbnailUrl, 64, 64),
                            'MediaType': 2,
                            'arryNumber': i,
                            'mediaId': data.items[i].mediaId,
                            'netVote': data.items[i].netVote,
                            'userId': data.items[i].userId,
                            'hasVoted': data.items[i].hasVoted,
                            'reviewPointScore': data.items[i].reviewPointScore,
                            'created': data.items[i].created
                        };
                        vm.imagesVideos.push(thevideogoods);
                    }
                }
            }        
            vm.notify(function () {
                vm.filterimagesVideos = vm.imagesVideos;

            });

        }


        function _openLightboxModal(index) {
            vm.Lightbox.openModal(vm.imagesVideos, index);
        };


        function _OpenModel() {
            var modalInstance = vm.$uibModal.open({
                animation: true,
                templateUrl: 'AddMediaModal.html',
                controller: 'imageController as modalCtrl',
                resolve: {
                    placesId: function () {
                        return vm.theplaceId;
                    }
                }

            });
            modalInstance.result.then(_modalYes, _modalNo);
        }

        function _OpenVideoModel() {
            var newmodalInstance = vm.$uibModal.open({
                animation: true,
                templateUrl: 'AddMediaVideoModal.html',
                controller: 'videoUploadController as vidmodalCtrl',
                resolve: {
                    placesId: function () {
                        return vm.theplaceId;
                    }
                }
            });
            newmodalInstance.result.then(_modalYes, _modalNo);
        }

        function _modalYes() {

            vm.$alertService.success("Media Submitted!");

            vm.loadMedia();
        }
        function _modalNo() {
            vm.loadMedia();

        }

        function _upVote(imagevideo) {
            vm.postVote.UserId = imagevideo.userId;
            vm.postVote.NetVote = 1;
            vm.postVote.ContentId = imagevideo.mediaId;
            vm.postVote.PlacesId = vm.theplaceId;
            // Vote can only be submitted once per user and is permanent
            if (!imagevideo.hasVoted) {
                vm.$voteService.insert(vm.postVote, vm.loadMedia, vm.voteError);

                vm.$alertService.success("Vote Submitted!");

            } else {
                vm.$alertService.error("You have already Voted!");

            }
        }
        function _downVote(imagevideo) {
            vm.postVote.UserId = imagevideo.userId;
            vm.postVote.NetVote = -1;
            vm.postVote.ContentId = imagevideo.mediaId;
            vm.postVote.PlacesId = vm.theplaceId;
            // Vote can only be submitted once per user and is permanent
            if (!imagevideo.hasVoted) {
                vm.$voteService.insert(vm.postVote, vm.loadMedia, vm.voteError);

                vm.$alertService.success("Vote Submitted!");

            } else {
                vm.$alertService.error("You have already Voted!");

            }
        }

        function _pageChanged() {
            vm.$placesService.getMediaByplacesId(vm.theplaceId, vm.CurrentPage, vm.PerPage, vm.renderSuccess);
        }
    }

})();
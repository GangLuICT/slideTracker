var slideTrack = angular.module('slideTrack', []);

//get requested presentation ID
var url = $(location).attr('href').split('/').splice(0, 5).join('/');
var socket = io('http://localhost', { query: "pres_ID="+url.substr(url.lastIndexOf('/') + 1) });

slideTrack.controller('mainController', function ($scope, $http) {

//function mainController($scope, $http) {

	$scope.slide_alt_text = 'loading...';
	$scope.cur_slide = 0;
	$scope.presentation = {};
	$scope.presentation.n_slides = 0;
	$scope.tracking = true;
	$scope.bFs = false;

	//get requested presentation ID
	$scope.pres_ID = url.substr(url.lastIndexOf('/') + 1);

	$scope.track = function() {
		$scope.tracking = true;
		$('#btn-track').html('tracking...');
		$('#btn-track').attr('disabled', 'disabled');
		$('#fs-btn-track').html('tracking...');
		$('#fs-btn-track').attr('disabled', 'disabled');
		$scope.current();
	};

	// go to current slide
	$scope.current = function() {
		$http.get('/api/v1/presentations/' + $scope.pres_ID).success(function(data) {
			if (!data[0]) {
				$scope.notFound();
			} else {
				$scope.presentation = data[0];
				if (!$scope.presentation.active) {
					$scope.slide_alt_text = 'presentation not ready';
				} else {
					$scope.cur_slide = $scope.presentation.cur_slide;
					$scope.slide_src = '/files/' + $scope.pres_ID + '/Slide' + $scope.cur_slide + '.PNG';
					$('#next-slide').removeAttr('disabled', 'disabled');
					$('#prev-slide').removeAttr('disabled', 'disabled');
					$('#fs-next-slide').removeAttr('disabled', 'disabled');
					$('#fs-prev-slide').removeAttr('disabled', 'disabled');
					$('#fs-btn').removeAttr('disabled', 'disabled');
					if (parseInt($scope.cur_slide) >= $scope.presentation.n_slides) {
						$('#next-slide').attr('disabled', 'disabled');
						$('#fs-next-slide').attr('disabled', 'disabled');
					}
					if (parseInt($scope.cur_slide) <= 1) {
						$('#prev-slide').attr('disabled', 'disabled');
						$('#fs-prev-slide').attr('disabled', 'disabled');
					}
					$scope.slide_alt_text = '';
					$('#tracking-box .slide-box').css('display','inline-block');
					$('#btn-track').html('tracking...');
					$('#btn-track').attr('disabled', 'disabled');
					$('#fs-btn-track').html('tracking...');
					$('#fs-btn-track').attr('disabled', 'disabled');
					//check if download available
					var file_url = '/files/'+$scope.pres_ID+'/presentation.pdf';	
				    if ($scope.presentation.download){
						$('#download-pres').attr('href',file_url);
						$('#download-pres').css('display','block');	
				    }
				}
			}
		}).error(function(data) {
			console.log('Error: ' + data);
		});
	};

	// delete presentation function
	$scope.quit = function() {
		$scope.cur_slide = 0;
		$scope.presentation.n_slides = 0;
		$scope.slide_alt_text = 'presentation finished';
		$('#tracking-box .slide-box').css('display','none');
		$('#download-pres').css('display','none');	
		$('#fs-btn').attr('disabled', 'disabled');
		$scope.slide_src = '';
		$scope.$apply();
		$scope.fs_exit();
	};

	//create presentation not found behaviour
	$scope.notFound = function() {
		$('#tracking-box').css('display', 'none');
		$('#pres-not-found').css('display', 'block');
		$('#fp-code-input input').val($scope.pres_ID);
		$('#fp-code-input input').focus();
		$('#submit-code').click(function() {
			var code = $('#fp-code-input input').val();
			if (code) {
				window.location.href = '/track/' + code;
			} else {
				$('#fp-code-input input').focus()
			}
		});

		//make sure enter submits form
		$('#fp-code-input input').bind('enterKey', function(e) {
			var code = $('#fp-code-input input').val();
			if (code) {
				window.location.href = '/track/' + code;
			} else {
				$('#fp-code-input input').focus()
			}
		});
		$('#fp-code-input input').keyup(function(e) {
			if (e.keyCode == 13) {
				$(this).trigger('enterKey');
			}
		});
	};

	// go to next slide
	$scope.next = function() {
		$http.get('/api/v1/presentations/' + $scope.pres_ID).success(function(data) {
			$scope.tracking = false;
			$('#btn-track').html('track');
			$('#btn-track').removeAttr('disabled', 'disabled');
			$('#fs-btn-track').html('track');
			$('#fs-btn-track').removeAttr('disabled', 'disabled');
			if (!data[0]) {
				$scope.slide_alt_text = 'presentation not found';
			} else {
				$scope.presentation = data[0];
				if (!$scope.presentation.active) {
					$scope.slide_alt_text = 'presentation not ready';
				} else {
					//check if we are at end of presentation
					if (parseInt($scope.cur_slide) + 1 >= $scope.presentation.n_slides) {
						$('#next-slide').attr('disabled', 'disabled');
						$('#fs-next-slide').attr('disabled', 'disabled');
						$scope.cur_slide = $scope.presentation.n_slides;
						$scope.slide_src = '/files/' + $scope.pres_ID + '/Slide' + $scope.cur_slide + '.PNG';
					} else {
						$scope.cur_slide = parseInt($scope.cur_slide) + 1;
						$scope.slide_src = '/files/' + $scope.pres_ID + '/Slide' + $scope.cur_slide + '.PNG';
					}
					$('#prev-slide').removeAttr('disabled', 'disabled');
					$('#fs-prev-slide').removeAttr('disabled', 'disabled');
					$scope.slide_alt_text = '';
					$('#tracking-box .slide-box').css('display','inline-block');
				}
			}
			$("#fs-tracking-box .fs-btn").css('display','');
			setTimeout('$("#fs-tracking-box .fs-btn").fadeOut();', 2000);
		}).error(function(data) {
			console.log('Error: ' + data);
		});
	};

	// go to previous slide
	$scope.previous = function() {
		$http.get('/api/v1/presentations/' + $scope.pres_ID).success(function(data) {
			$scope.tracking = false;
			$('#btn-track').html('track');
			$('#btn-track').removeAttr('disabled', 'disabled');
			$('#fs-btn-track').html('track');
			$('#fs-btn-track').removeAttr('disabled', 'disabled');
			if (!data[0]) {
				$scope.slide_alt_text = 'presentation not found';
			} else {
				$scope.presentation = data[0];
				if (!$scope.presentation.active) {
					$scope.slide_alt_text = 'presentation not ready';
				} else {
					//check if we are at beginning of presentation
					if (parseInt($scope.cur_slide) - 1 <= 1) {
						$('#prev-slide').attr('disabled', 'disabled');
						$('#fs-prev-slide').attr('disabled', 'disabled');
						$scope.cur_slide = 1;
						$scope.slide_src = '/files/' + $scope.pres_ID + '/Slide' + $scope.cur_slide + '.PNG';
					} else {
						$scope.cur_slide = parseInt($scope.cur_slide) - 1;
						$scope.slide_src = '/files/' + $scope.pres_ID + '/Slide' + $scope.cur_slide + '.PNG';
					}
					$('#next-slide').removeAttr('disabled', 'disabled');
					$('#fs-next-slide').removeAttr('disabled', 'disabled');					
					$scope.slide_alt_text = '';
					$('#tracking-box .slide-box').css('display','inline-block');
				}
			}
			$("#fs-tracking-box .fs-btn").css('display','');
			setTimeout('$("#fs-tracking-box .fs-btn").fadeOut();', 2000);
		}).error(function(data) {
			console.log('Error: ' + data);
		});
	};
	
	// go to fullscreen
	$scope.fullscreen = function() {
		
		$('#fs-tracking-box .fs-btn').hide();		
		$('#fs-tracking-box').css('display', 'block');
		$scope.bFs = true;
		
		$scope.fs_position();

		$('#fs-tracking-box .fs-btn').css('display','');
		setTimeout('$("#fs-tracking-box .fs-btn").fadeOut();', 2000);
		
	};

	// exit fullscreen
	$scope.fs_exit = function() {
		$('#fs-tracking-box').css('display', 'none');
		$scope.bFs = false;
	};
	
	// position fullscreen buttons
	$scope.fs_position = function() {
		var fsImg = document.getElementById('fs-img'); 
		var fsImgW = fsImg.clientWidth;
		var fsImgH = fsImg.clientHeight;
		var fsWindW = $( window ).width();
		var fsWindH = $( window ).height();
		var bTop = ((fsWindH-fsImgH)/2+10);
		var bRight = ((fsWindW-fsImgW)/2+10);
		var bLeft = ((fsWindW-fsImgW)/2+10);
		$('#fs-btn-track').css('top',bTop+'px');
		$('#fs-prev-slide').css('left',bLeft+'px');
		$('#fs-next-slide').css('right',bRight+'px');
		$('#fs-exit').css('top',bTop+'px');
		$('#fs-exit').css('right',bRight+'px');
	};
	
	// track presentation
	socket.on('update', function(pres_ID) {
		if (pres_ID == $scope.pres_ID && $scope.tracking) {
			$scope.current();
		}
	});

	// delete presentation
	socket.on('quit', function(pres_ID) {
		if (pres_ID == $scope.pres_ID) {
			$('#next-slide').attr('disabled', 'disabled');
			$('#prev-slide').attr('disabled', 'disabled');
			$('#btn-track').html('done');
			$('#btn-track').attr('disabled', 'disabled');
			$('#fs-next-slide').attr('disabled', 'disabled');
			$('#fs-prev-slide').attr('disabled', 'disabled');
			$('#fs-btn-track').html('done');
			$('#fs-btn-track').attr('disabled', 'disabled');
			$scope.quit();
		}
	});

	//if in fullscreen mode adjust button position when resizing window
	$( window ).resize(function() {
		if($scope.bFs){
			$scope.fs_position();
		}
	});
	
	//make keys work for navigation
	$('body').keydown(function(e) {
		if(e.keyCode == 37) { // left
			if($scope.bFs){
				$('#fs-prev-slide').click();
			}
		}
		else if(e.keyCode == 39) { // right
			if($scope.bFs){
				$('#fs-next-slide').click();
			}
		}
	});
		
	// start tracking on initial load
	$scope.current();


//}
});



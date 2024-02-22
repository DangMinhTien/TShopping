// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$('.m-toast-box').on('click', '.m-toast-close', function () {
    console.log($(this))
    $(this).closest('.m-toast-message').hide()
})
$('.m-toast-message').show(function () {
    var toast = $(this)
    setTimeout(function () {
        toast.hide()
    }, 5000)
})
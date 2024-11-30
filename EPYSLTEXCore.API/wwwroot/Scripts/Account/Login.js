$(function () {

    loadProgressBar();

    $("#btn-signin").on('click', function (e) {
        $(this).addClass("disabled");
        e.preventDefault();

        var data = $("#login-form").serialize();
    
        axios.post('/account/login', data)

            .then(function (response) {

                localStorage.setItem("token", response.data.accessToken);
                window.location.href = "/dashboard/index";
            })
            .catch(function () {
                toastr.error('Invalid username or password!', 'Error');
                $("#btn-signin").removeClass("disabled");
            });
    });






});
document.addEventListener('contextmenu', (e) => e.preventDefault());
document.onkeydown = (e) => {
    // Disable F12, Ctrl + Shift + I, Ctrl + Shift + J, Ctrl + U
    if (
        event.keyCode === 123 ||
        ctrlShiftKey(e, 'I') ||
        ctrlShiftKey(e, 'J') ||
        ctrlShiftKey(e, 'C') ||
        (e.ctrlKey && e.keyCode === 'U'.charCodeAt(0))
    )
        return false;
};

function ctrlShiftKey(e, keyCode) {
    return e.ctrlKey && e.shiftKey && e.keyCode === keyCode.charCodeAt(0);
}
function changeForm(change) {

    if (change == 0) {

        $("#forgot-password").show();
        $("#forgot-span").show();

        $("#sign-in").hide();
        $("#sign-in-span").hide();
    }
    else {
        $("#sign-in").show();
        $("#sign-in-span").show();

        $("#forgot-span").hide();
        $("#forgot-password").hide();
    }

}
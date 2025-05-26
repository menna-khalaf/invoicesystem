//// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
//// for details on configuring this project to bundle and minify static web assets.

//// Write your JavaScript code.
//// Custom JavaScript for the Create Branch form
//document.addEventListener('DOMContentLoaded', function () {
//    const form = document.getElementById('createBranchForm');

//    // Add validation feedback on form submission
//    form.addEventListener('submit', function (e) {
//        if (!form.checkValidity()) {
//            e.preventDefault();
//            e.stopPropagation();
//        }
//        form.classList.add('was-validated');
//    });

//    // Add real-time validation feedback on input change
//    const inputs = form.querySelectorAll('.form-control, .form-select');
//    inputs.forEach(input => {
//        input.addEventListener('input', function () {
//            if (input.checkValidity()) {
//                input.classList.remove('is-invalid');
//                input.classList.add('is-valid');
//            } else {
//                input.classList.remove('is-valid');
//                input.classList.add('is-invalid');
//            }
//        });
//    });

//    // Add animation to the submit button
//    const submitButton = form.querySelector('button[type="submit"]');
//    submitButton.addEventListener('mouseenter', function () {
//        submitButton.style.transform = 'scale(1.05)';
//    });
//    submitButton.addEventListener('mouseleave', function () {
//        submitButton.style.transform = 'scale(1)';
//    });
//});
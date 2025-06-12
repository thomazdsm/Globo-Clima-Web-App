// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-dismiss alerts after 5 seconds
document.addEventListener("DOMContentLoaded", () => {
    const alerts = document.querySelectorAll(".alert-dismissible")
    alerts.forEach((alert) => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert)
            bsAlert.close()
        }, 5000)
    })
})

// Form validation enhancements
document.addEventListener("DOMContentLoaded", () => {
    const forms = document.querySelectorAll("form")
    forms.forEach((form) => {
        form.addEventListener("submit", (event) => {
            const submitButton = form.querySelector('button[type="submit"]')
            if (submitButton) {
                submitButton.disabled = true
                const originalText = submitButton.textContent
                submitButton.textContent = "Processando..."

                // Re-enable button after 5 seconds as fallback
                setTimeout(() => {
                    submitButton.disabled = false
                    submitButton.textContent = originalText
                }, 5000)
            }
        })
    })
})

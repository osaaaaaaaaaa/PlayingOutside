FROM php:8.3.7-apache
# PDOを使用できるようにする
RUN docker-php-ext-install pdo_mysql

# ドキュメントルートを変更する
ENV APACHE_DOCUMENT_ROOT='/var/www/public/'
RUN sed -ri -e 's!/var/www/html!${APACHE_DOCUMENT_ROOT}!g' /etc/apache2/sites-available/*.conf

# xdebugの使用
RUN pecl install xdebug && \
    docker-php-ext-enable xdebug
COPY docker-php-ext-xdebug.ini /usr/local/etc/php/conf.d/docker-php-ext-xdebug.ini

# Laravelで必要になるmodRewriteを有効化する
RUN mv /etc/apache2/mods-available/rewrite.load /etc/apache2/mods-enabled
RUN /bin/sh -c a2enmod rewrite

COPY --from=composer:2.6.6 /usr/bin/composer /usr/bin/composer
COPY apache2.conf /etc/apache2/apache2.conf
COPY php.ini /usr/local/etc/php/php.ini

COPY src/ /var/www
RUN chmod 777 -R /var/www/storage

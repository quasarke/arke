FROM debian:stretch-slim

ENV ASTERISK_VERSION 18.1.1

WORKDIR /stretch
COPY start.sh /root/start.sh

RUN apt update && \
    apt install -y git curl wget libnewt-dev libssl-dev libncurses5-dev libsqlite3-dev build-essential libjansson-dev libxml2-dev uuid-dev libedit-dev mpg123 ffmpeg subversion uuid-runtime \
    && export GNUPGHOME="$(mktemp -d)" \
  && wget https://downloads.asterisk.org/pub/telephony/asterisk/releases/asterisk-${ASTERISK_VERSION}.tar.gz \
  && tar xzf asterisk-${ASTERISK_VERSION}.tar.gz \
  && cd asterisk-${ASTERISK_VERSION} \
  && useradd --system asterisk \
  && ./configure --with-pjproject-bundled --with-jansson-bundled \
  && make menuselect.makeopts \
  && ./menuselect/menuselect \
    --disable BUILD_NATIVE \
# Addons
    --enable chan_mobile \
    --disable chan_ooh323 \
    --enable format_mp3 \
    --disable res_config_mysql \
    --disable app_mysql \
    --disable cdr_mysql \
# Apps
    --enable-category MENUSELECT_APPS \
    --disable app_skel \
    --disable app_fax \
    --disable app_ivrdemo \
    --disable app_saycounted \
    --disable app_statsd \
# Bridges
    --enable-category MENUSELECT_BRIDGES \
# CDR
    --enable-category MENUSELECT_CDR \
    --disable cdr_pgsql \
    --disable cdr_radius \
    --disable cdr_custom \
    --disable cdr_csv \
    --disable cdr_sqlite3_custom \
    --disable cdr_syslog \
# CEL
    --enable-category MENUSELECT_CEL \
    --disable cel_pgsql \
    --disable cel_radius \
    --disable cel_custom \
    --disable cel_manager \
    --disable cel_sqlite3_custom \
# Channels
    --enable-category MENUSELECT_CHANNELS \
# Codecs
    --enable-category MENUSELECT_CODECS \
# Formats
    --enable-category MENUSELECT_FORMATS \
# Funcs
    --enable-category MENUSELECT_FUNCS \
# PBX
    --enable-category MENUSELECT_PBX \
    --enable pbx_lua \
# RES
    --enable-category MENUSELECT_RES \
    --disable res_mwi_external \
    --disable res_chan_stats \
    --disable res_endpoint_stats \
    --disable res_pktccops \
# Tests
    --enable-category MENUSELECT_TESTS \
# MENUSELECT_OPTS_app_voicemail
    --enable FILE_STORAGE \
    --disable ODBC_STORAGE \
    --disable IMAP_STORAGE \
# Utils
    --enable-category MENUSELECT_UTILS \
    --disable aelparse \
    --disable astman \
    --disable check_expr \
    --disable check_expr2 \
    --disable conf2ael \
    --disable muted \
    --disable smsq \
    --disable stereorize \
    --enable  streamplayer \
    --disable astdb2sqlite3 \
    --disable astdb2bdb \
# AGIS
    --disable-category MENUSELECT_AGIS \
# Core sounds
    --disable-category MENUSELECT_CORE_SOUNDS \
    --enable CORE-SOUNDS-EN-ULAW \
# MOH
    --disable-category MENUSELECT_MOH \
    --enable MOH-OPSOUND-ULAW \
# Extra Sounds
    --disable-category MENUSELECT_EXTRA_SOUNDS \
    --enable EXTRA-SOUNDS-EN-ULAW \
    menuselect.makeopts \
&& make -j $(nproc) \
&& sh contrib/scripts/get_mp3_source.sh \
&& make install \
&& devpackages=`dpkg -l|grep '\-dev'|awk '{print $2}'|xargs` \
&& DEBIAN_FRONTEND=noninteractive apt-get --yes purge --auto-remove \
  autoconf \
  build-essential \
  git \
  subversion \
  bzip2 \
  cpp \
  m4 \
  make \
  patch \
  perl \
  perl-modules \
  pkg-config \
  xz-utils \
  ${devpackages} \
&& rm -rf /var/lib/apt/lists/* \
&& mkdir -p /etc/asterisk/ \
         /var/spool/asterisk/fax \
&& chown -R asterisk:asterisk /etc/asterisk \
                           /var/*/asterisk \
                           /usr/*/asterisk \
&& chmod -R 750 /var/spool/asterisk \
&& cd .. \
&& rm * -R

EXPOSE 5060/udp
VOLUME /var/lib/asterisk /etc/asterisk

ENTRYPOINT [ "/root/start.sh" ]